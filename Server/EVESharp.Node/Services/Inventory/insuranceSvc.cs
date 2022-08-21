using System;
using EVESharp.Database;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Data.Market;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Exceptions.insuranceSvc;
using EVESharp.EVE.Exceptions.jumpCloneSvc;
using EVESharp.EVE.Market;
using EVESharp.EVE.Network;
using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Chat;
using EVESharp.Node.Database;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Inventory;

[MustBeCharacter]
public class insuranceSvc : ClientBoundService
{
    private readonly int                 mStationID;
    public override  AccessLevel         AccessLevel  => AccessLevel.None;
    private          InsuranceDB         DB           { get; }
    private          IItems              Items        { get; }
    private          MarketDB            MarketDB     { get; }
    private          ISolarSystems       SolarSystems { get; }
    private          IWallets            Wallets      { get; }
    private          MailManager         MailManager  { get; }
    private          IDatabaseConnection Database     { get; }

    public insuranceSvc
    (
        IClusterManager clusterManager, IItems items, InsuranceDB db, MarketDB marketDB, IWallets wallets, MailManager mailManager, BoundServiceManager manager,
        IDatabaseConnection database,
        ISolarSystems solarSystems
    ) : base (manager)
    {
        DB           = db;
        Items        = items;
        MarketDB     = marketDB;
        this.Wallets = wallets;
        MailManager  = mailManager;
        Database     = database;
        SolarSystems = solarSystems;

        clusterManager.ClusterTimerTick += PerformTimedEvents;
    }

    protected insuranceSvc
    (
        IItems items,     InsuranceDB db,      MarketDB      marketDB, IWallets wallets, MailManager mailManager, BoundServiceManager manager,
        int    stationID, Session     session, ISolarSystems solarSystems
    ) : base (manager, session, stationID)
    {
        this.mStationID = stationID;
        DB              = db;
        Items           = items;
        MarketDB        = marketDB;
        this.Wallets    = wallets;
        MailManager     = mailManager;
        SolarSystems    = solarSystems;
    }

    public PyList <PyPackedRow> GetContracts (CallInformation call)
    {
        if (this.mStationID == 0)
        {
            int? shipID = call.Session.ShipID;

            if (shipID is null)
                throw new CustomError ("The character is not onboard any ship");

            return new PyList <PyPackedRow> (1) {[0] = DB.GetContractForShip (call.Session.CharacterID, (int) shipID)};
        }

        return DB.GetContractsForShipsOnStation (call.Session.CharacterID, this.mStationID);
    }

    public PyPackedRow GetContractForShip (CallInformation call, PyInteger itemID)
    {
        return DB.GetContractForShip (call.Session.CharacterID, itemID);
    }

    public PyList <PyPackedRow> GetContracts (CallInformation call, PyInteger includeCorp)
    {
        if (includeCorp == 0)
            return DB.GetContractsForShipsOnStation (call.Session.CharacterID, this.mStationID);

        return DB.GetContractsForShipsOnStationIncludingCorp (call.Session.CharacterID, call.Session.CorporationID, this.mStationID);
    }

    public PyBool InsureShip (CallInformation call, PyInteger itemID, PyDecimal insuranceCost, PyInteger isCorpItem)
    {
        int callerCharacterID = call.Session.CharacterID;

        if (this.Items.TryGetItem (itemID, out Ship item) == false)
            throw new CustomError ("Ships not loaded for player and hangar!");

        Character character = this.Items.GetItem <Character> (callerCharacterID);

        if (isCorpItem == 1 && item.OwnerID != call.Session.CorporationID && item.OwnerID != callerCharacterID)
            throw new MktNotOwner ();

        if (item.Singleton == false)
            throw new InsureShipFailed ("Only assembled ships can be insured");

        if (DB.IsShipInsured (item.ID, out int oldOwnerID, out int numberOfInsurances) &&
            (call.NamedPayload.TryGetValue ("voidOld", out PyBool voidOld) == false || voidOld == false))
        {
            // throw the proper exception based on the number of insurances available
            if (numberOfInsurances > 1)
                throw new InsureShipFailedMultipleContracts ();

            throw new InsureShipFailedSingleContract (oldOwnerID);
        }

        using IWallet wallet = this.Wallets.AcquireWallet (character.ID, WalletKeys.MAIN);

        {
            wallet.EnsureEnoughBalance (insuranceCost);
            wallet.CreateJournalRecord (MarketReference.Insurance, this.Items.OwnerSCC.ID, -item.ID, -insuranceCost, $"Insurance fee for {item.Name}");
        }

        // insurance was charged to the player, so old insurances can be void now
        DB.UnInsureShip (item.ID);

        double fraction = insuranceCost * 100 / item.Type.BasePrice;

        // create insurance record
        DateTime expirationTime = DateTime.UtcNow.AddDays (7 * 12);
        int      referenceID    = DB.InsureShip (item.ID, isCorpItem == 0 ? callerCharacterID : call.Session.CorporationID, fraction / 5, expirationTime);

        // TODO: CHECK IF THE INSURANCE SHOULD BE CHARGED TO THE CORP

        MailManager.SendMail (
            this.Items.OwnerSCC.ID, callerCharacterID,
            "Insurance Contract Issued",
            "Dear valued customer, <br><br>" +
            "Congratulations on the insurance on your ship. A very wise choice indeed.<br>" +
            $"This letter is to confirm that we have issued an insurance contract for your ship, <b>{item.Name}</b> (<b>{item.Type.Name}</b>) at a level of {fraction * 100 / 30}%.<br>" +
            $"This contract will expire at <b>{expirationTime.ToLongDateString ()} {expirationTime.ToShortTimeString ()}</b>, after 12 weeks.<br><br>" +
            "Best,<br>" +
            "The Secure Commerce Commission<br>" +
            $"Reference ID: <b>{referenceID}</b>"
        );

        return true;
    }

    public PyDataType UnInsureShip (CallInformation call, PyInteger itemID)
    {
        int callerCharacterID = call.Session.CharacterID;

        if (this.Items.TryGetItem (itemID, out Ship item) == false)
            throw new CustomError ("Ships not loaded for player and hangar!");

        if (item.OwnerID != call.Session.CorporationID && item.OwnerID != callerCharacterID)
            throw new MktNotOwner ();

        // remove insurance record off the database
        DB.UnInsureShip (itemID);

        return null;
    }

    public void PerformTimedEvents (object sender, EventArgs args)
    {
        foreach (InsuranceDB.ExpiredContract contract in DB.GetExpiredContracts ())
        {
            DateTime insuranceTime = DateTime.FromFileTimeUtc (contract.StartDate);

            MailManager.SendMail (
                this.Items.OwnerSCC.ID, contract.OwnerID,
                "Insurance Contract Expired",
                "Dear valued customer, <br><br>" +
                $"The insurance contract between yourself and SCC for the insurance of the ship <b>{contract.ShipName}</b> (<b>{contract.ShipType.Name}</b>) issued at" +
                $" <b>{insuranceTime.ToLongDateString ()} {insuranceTime.ToShortTimeString ()}</b> has expired." +
                "Please purchase a new insurance as quickly as possible to protect your investment.<br><br>" +
                "Best,<br>" +
                "The Secure Commerce Commission<br>" +
                $"Reference ID: <b>{contract.InsuranceID}</b>"
            );
        }
    }

    protected override long MachoResolveObject (CallInformation call, ServiceBindParams parameters)
    {
        return Database.CluResolveAddress ("solarsystem", parameters.ObjectID);
    }

    protected override BoundService CreateBoundInstance (CallInformation call, ServiceBindParams bindParams)
    {
        if (this.MachoResolveObject (call, bindParams) != BoundServiceManager.MachoNet.NodeID)
            throw new CustomError ("Trying to bind an object that does not belong to us!");

        return new insuranceSvc (
            this.Items, DB, MarketDB, this.Wallets, MailManager, BoundServiceManager, bindParams.ObjectID, call.Session,
            this.SolarSystems
        );
    }
}