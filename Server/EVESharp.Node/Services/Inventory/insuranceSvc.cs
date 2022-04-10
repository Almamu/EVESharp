using System;
using EVESharp.EVE.Client.Exceptions.insuranceSvc;
using EVESharp.EVE.Client.Exceptions.jumpCloneSvc;
using EVESharp.EVE.Market;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
using EVESharp.EVE.Sessions;
using EVESharp.EVE.Wallet;
using EVESharp.Node.Chat;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Market;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Inventory;

[MustBeCharacter]
public class insuranceSvc : ClientBoundService
{
    private readonly int           mStationID;
    public override  AccessLevel   AccessLevel   => AccessLevel.None;
    private          InsuranceDB   DB            { get; }
    private          ItemFactory   ItemFactory   { get; }
    private          MarketDB      MarketDB      { get; }
    private          SystemManager SystemManager => ItemFactory.SystemManager;
    private          WalletManager WalletManager { get; }
    private          MailManager   MailManager   { get; }

    public insuranceSvc (
        ItemFactory itemFactory, InsuranceDB db, MarketDB marketDB, WalletManager walletManager, MailManager mailManager, BoundServiceManager manager
    ) : base (manager)
    {
        DB            = db;
        ItemFactory   = itemFactory;
        MarketDB      = marketDB;
        WalletManager = walletManager;
        MailManager   = mailManager;

        // TODO: RE-IMPLEMENT ON CLUSTER TIMER
        // machoNet.OnClusterTimer += PerformTimedEvents;
    }

    protected insuranceSvc (
        ItemFactory itemFactory, InsuranceDB db, MarketDB marketDB, WalletManager walletManager, MailManager mailManager, BoundServiceManager manager,
        int         stationID,   Session     session
    ) : base (manager, session, stationID)
    {
        this.mStationID = stationID;
        DB              = db;
        ItemFactory     = itemFactory;
        MarketDB        = marketDB;
        WalletManager   = walletManager;
        MailManager     = mailManager;
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

    public PyPackedRow GetContractForShip (PyInteger itemID, CallInformation call)
    {
        return DB.GetContractForShip (call.Session.CharacterID, itemID);
    }

    public PyList <PyPackedRow> GetContracts (PyInteger includeCorp, CallInformation call)
    {
        if (includeCorp == 0)
            return DB.GetContractsForShipsOnStation (call.Session.CharacterID, this.mStationID);

        return DB.GetContractsForShipsOnStationIncludingCorp (call.Session.CharacterID, call.Session.CorporationID, this.mStationID);
    }

    public PyBool InsureShip (PyInteger itemID, PyDecimal insuranceCost, PyInteger isCorpItem, CallInformation call)
    {
        int callerCharacterID = call.Session.CharacterID;

        if (ItemFactory.TryGetItem (itemID, out Ship item) == false)
            throw new CustomError ("Ships not loaded for player and hangar!");

        Character character = ItemFactory.GetItem <Character> (callerCharacterID);

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

        using Wallet wallet = WalletManager.AcquireWallet (character.ID, Keys.MAIN);
        {
            wallet.EnsureEnoughBalance (insuranceCost);
            wallet.CreateJournalRecord (MarketReference.Insurance, ItemFactory.OwnerSCC.ID, -item.ID, -insuranceCost, $"Insurance fee for {item.Name}");
        }

        // insurance was charged to the player, so old insurances can be void now
        DB.UnInsureShip (item.ID);

        double fraction = insuranceCost * 100 / item.Type.BasePrice;

        // create insurance record
        DateTime expirationTime = DateTime.UtcNow.AddDays (7 * 12);
        int      referenceID    = DB.InsureShip (item.ID, isCorpItem == 0 ? callerCharacterID : call.Session.CorporationID, fraction / 5, expirationTime);

        // TODO: CHECK IF THE INSURANCE SHOULD BE CHARGED TO THE CORP

        MailManager.SendMail (
            ItemFactory.OwnerSCC.ID, callerCharacterID,
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

    public PyDataType UnInsureShip (PyInteger itemID, CallInformation call)
    {
        int callerCharacterID = call.Session.CharacterID;

        if (ItemFactory.TryGetItem (itemID, out Ship item) == false)
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
                ItemFactory.OwnerSCC.ID, contract.OwnerID,
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

    protected override long MachoResolveObject (ServiceBindParams parameters, CallInformation call)
    {
        int solarSystemID = ItemFactory.GetStaticStation (parameters.ObjectID).SolarSystemID;

        if (SystemManager.SolarSystemBelongsToUs (solarSystemID))
            return BoundServiceManager.MachoNet.NodeID;

        return SystemManager.GetNodeSolarSystemBelongsTo (solarSystemID);
    }

    protected override BoundService CreateBoundInstance (ServiceBindParams bindParams, CallInformation call)
    {
        if (this.MachoResolveObject (bindParams, call) != BoundServiceManager.MachoNet.NodeID)
            throw new CustomError ("Trying to bind an object that does not belong to us!");

        return new insuranceSvc (ItemFactory, DB, MarketDB, WalletManager, MailManager, BoundServiceManager, bindParams.ObjectID, call.Session);
    }
}