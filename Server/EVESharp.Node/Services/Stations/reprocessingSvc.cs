using System;
using System.Collections.Generic;
using EVESharp.Database;
using EVESharp.Database.Extensions;
using EVESharp.Database.Inventory;
using EVESharp.Database.Inventory.Types;
using EVESharp.Database.Old;
using EVESharp.Database.Types;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Dogma;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Exceptions.jumpCloneSvc;
using EVESharp.EVE.Exceptions.reprocessingSvc;
using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Network.Services.Validators;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Notifications.Inventory;
using EVESharp.EVE.Sessions;
using EVESharp.EVE.Types;
using EVESharp.Types;
using EVESharp.Types.Collections;
using Service = EVESharp.Database.Inventory.Stations.Service;

namespace EVESharp.Node.Services.Stations;

[MustBeCharacter]
public class reprocessingSvc : ClientBoundService
{
    private static readonly Dictionary <TypeID, TypeID> sOreTypeIDtoProcessingSkillTypeID = new Dictionary <TypeID, TypeID>
    {
        {TypeID.Arkonor, TypeID.ArkonorProcessing},
        {TypeID.Bistot, TypeID.BistotProcessing},
        {TypeID.Crokite, TypeID.CrokiteProcessing},
        {TypeID.DarkOchre, TypeID.DarkOchreProcessing},
        {TypeID.Gneiss, TypeID.GneissProcessing},
        {TypeID.Hedbergite, TypeID.HedbergiteProcessing},
        {TypeID.Hemorphite, TypeID.HemorphiteProcessing},
        {TypeID.Jaspet, TypeID.JaspetProcessing},
        {TypeID.Kernite, TypeID.KerniteProcessing},
        {TypeID.Mercoxit, TypeID.MercoxitProcessing},
        {TypeID.Omber, TypeID.OmberProcessing},
        {TypeID.Plagioclase, TypeID.PlagioclaseProcessing},
        {TypeID.Pyroxeres, TypeID.PyroxeresProcessing},
        {TypeID.Scordite, TypeID.ScorditeProcessing},
        {TypeID.Spodumain, TypeID.SpodumainProcessing},
        {TypeID.Veldspar, TypeID.VeldsparProcessing}
    };
    private readonly double        mTaxRate;
    private readonly ItemInventory mInventory;
    private readonly Station       mStation;
    public override  AccessLevel   AccessLevel => AccessLevel.None;

    private IItems              Items              { get; }
    private ISolarSystems       SolarSystems       { get; }
    private ITypes              Types              => this.Items.Types;
    private StandingDB          StandingDB         { get; }
    private ReprocessingDB      ReprocessingDB     { get; }
    private IDogmaNotifications DogmaNotifications { get; }
    private IDatabase           Database           { get; }
    private IDogmaItems         DogmaItems         { get; }

    public reprocessingSvc
    (
        ReprocessingDB      reprocessingDb, StandingDB standingDb, IItems items, IBoundServiceManager manager, IDogmaNotifications dogmaNotifications,
        IDatabase database,
        ISolarSystems       solarSystems,
        IDogmaItems dogmaItems
    ) : base (manager)
    {
        ReprocessingDB     = reprocessingDb;
        StandingDB         = standingDb;
        Items              = items;
        DogmaNotifications = dogmaNotifications;
        Database           = database;
        SolarSystems       = solarSystems;
        DogmaItems         = dogmaItems;
    }

    protected reprocessingSvc
    (
        ReprocessingDB       reprocessingDb, StandingDB          standingDb, Station station, ItemInventory inventory, IItems items,
        IBoundServiceManager manager,        IDogmaNotifications dogmaNotifications, Session session, ISolarSystems solarSystems, IDogmaItems dogmaItems
    ) : base (manager, session, inventory.ID)
    {
        ReprocessingDB     = reprocessingDb;
        StandingDB         = standingDb;
        this.mStation      = station;
        this.mInventory    = inventory;
        Items              = items;
        DogmaNotifications = dogmaNotifications;
        SolarSystems       = solarSystems;
        DogmaItems         = dogmaItems;
        
        // TODO: LOAD TAXRATE FOR THE CORPORATION
    }

    private double CalculateCombinedYield (Character character)
    {
        // there's no implants that affect the reprocessing of anything
        double efficiency = 0.375
                            * (1 + 0.02 * character.GetSkillLevel (TypeID.Refining))
                            * (1 + 0.04 * character.GetSkillLevel (TypeID.RefineryEfficiency));

        efficiency += this.mStation.ReprocessingEfficiency;

        // efficiency should be maximum 1.0
        return Math.Min (efficiency, 1.0f);
    }

    private double CalculateEfficiency (Character character, int typeID)
    {
        // there's no implants that affect the reprocessing of anything
        double efficiency = 0.375
                            * (1 + 0.02 * character.GetSkillLevel (TypeID.Refining))
                            * (1 + 0.04 * character.GetSkillLevel (TypeID.RefineryEfficiency));

        // check what mineral it is and calculate it's efficiency (there's skills that modify the outcome) 
        if (sOreTypeIDtoProcessingSkillTypeID.TryGetValue ((TypeID) typeID, out TypeID skillType) == false)
            skillType = TypeID.ScrapmetalProcessing;

        // 5% increase by the specific metal skill
        efficiency *= 1 + 0.05 * character.GetSkillLevel (skillType);
        // finally take into account station's efficienfy
        efficiency += this.mStation.ReprocessingEfficiency;

        // efficiency should be maximum 1.0
        return Math.Min (efficiency, 1.0f);
    }

    private double CalculateTax (int corporationID, double standing)
    {
        return Math.Max (0.0f, Database.CrpGetTaxRate (corporationID) - 0.75 / 100.0 * standing);
    }

    private double GetStanding (Character character)
    {
        // TODO: TAKE THIS ONE OUT OF HERE AND INTO THE PROPER PART OF THE SYSTEM
        double standing = StandingDB.GetStanding (this.mStation.OwnerID, character.ID);

        if (standing < 0.0f)
            standing += (10.0 + standing) * 0.04 * character.GetSkillLevel (TypeID.Diplomacy);
        else
            standing += (10.0 - standing) * 0.04 * character.GetSkillLevel (TypeID.Connections);

        return standing;
    }

    [MustBeInStation]
    public PyDataType GetReprocessingInfo (ServiceCall call)
    {
        Character character = this.Items.GetItem <Character> (call.Session.CharacterID);

        double standing = this.GetStanding (character);

        return new PyDictionary
        {
            ["yield"]         = this.mStation.ReprocessingEfficiency,
            ["combinedyield"] = this.CalculateCombinedYield (character),
            ["wetake"] = new PyList (2)
            {
                [0] = this.CalculateTax (call.Session.CorporationID, standing),
                [1] = standing
            }
        };
    }

    private PyDataType GetQuote (Character character, ItemEntity item)
    {
        if (item.Quantity < item.Type.PortionSize)
            throw new QuantityLessThanMinimumPortion (item.Type);

        int leftovers         = item.Quantity % item.Type.PortionSize;
        int quantityToProcess = item.Quantity - leftovers;

        List <ReprocessingDB.Recoverables> recoverablesList = ReprocessingDB.GetRecoverables (item.Type.ID);
        double taxRate = Database.CrpGetTaxRate (character.CorporationID);

        Rowset recoverables = new Rowset (
            new PyList <PyString> (4)
            {
                [0] = "typeID",
                [1] = "unrecoverable",
                [2] = "station",
                [3] = "client"
            }
        );

        foreach (ReprocessingDB.Recoverables recoverable in recoverablesList)
        {
            int ratio = recoverable.AmountPerBatch * quantityToProcess / item.Type.PortionSize;

            double efficiency = this.CalculateEfficiency (character, recoverable.TypeID);

            recoverables.Rows.Add (
                new PyList (4)
                {
                    [0] = recoverable.TypeID,
                    [1] = (1.0 - efficiency) * ratio,
                    [2] = efficiency * taxRate * ratio,
                    [3] = efficiency * (1.0 - taxRate) * ratio
                }
            );
        }

        return new Row (
            new PyList <PyString> (4)
            {
                [0] = "leftOvers",
                [1] = "quantityToProcess",
                [2] = "playerStanding",
                [3] = "recoverables"
            },
            new PyList (4)
            {
                [0] = leftovers,
                [1] = quantityToProcess,
                [2] = this.GetStanding (character),
                [3] = recoverables
            }
        );
    }

    public PyDataType GetQuotes (ServiceCall call, PyList itemIDs)
    {
        Character character = this.Items.GetItem <Character> (call.Session.CharacterID);

        PyDictionary <PyInteger, PyDataType> result = new PyDictionary <PyInteger, PyDataType> ();

        foreach (PyInteger itemID in itemIDs.GetEnumerable <PyInteger> ())
        {
            if (this.mInventory.Items.TryGetValue (itemID, out ItemEntity item) == false)
                throw new MktNotOwner ();

            result [itemID] = this.GetQuote (character, item);
        }

        return result;
    }

    private void Reprocess (Character character, ItemEntity item, Session session)
    {
        if (item.Quantity < item.Type.PortionSize)
            throw new QuantityLessThanMinimumPortion (item.Type);

        int leftovers         = item.Quantity % item.Type.PortionSize;
        int quantityToProcess = item.Quantity - leftovers;

        List <ReprocessingDB.Recoverables> recoverablesList = ReprocessingDB.GetRecoverables (item.Type.ID);
        double taxRate = Database.CrpGetTaxRate (character.CorporationID);

        foreach (ReprocessingDB.Recoverables recoverable in recoverablesList)
        {
            int ratio = recoverable.AmountPerBatch * quantityToProcess / item.Type.PortionSize;

            double efficiency = this.CalculateEfficiency (character, recoverable.TypeID);

            int quantityForClient = (int) (efficiency * (1.0 - taxRate) * ratio);

            // create the new item
            ItemEntity newItem = this.Items.CreateSimpleItem (
                this.Types [recoverable.TypeID], character, this.mStation,
                Flags.Hangar, quantityForClient
            );

            // notify the client about the new item
            this.DogmaNotifications.QueueMultiEvent (session.CharacterID, OnItemChange.BuildNewItemChange (newItem));
        }
    }

    public PyDataType Reprocess (ServiceCall call, PyList itemIDs, PyInteger ownerID, PyInteger flag, PyBool unknown, PyList skipChecks)
    {
        Character character = this.Items.GetItem <Character> (call.Session.CharacterID);

        // TODO: TAKE INTO ACCOUNT OWNERID AND FLAG, THESE MOST LIKELY WILL BE USED BY CORP STUFF
        foreach (PyInteger itemID in itemIDs.GetEnumerable <PyInteger> ())
        {
            if (this.mInventory.Items.TryGetValue (itemID, out ItemEntity item) == false)
                throw new MktNotOwner ();

            // reprocess the item
            this.Reprocess (character, item, call.Session);

            // destroy the item
            DogmaItems.DestroyItem (item);
        }

        return null;
    }

    protected override long MachoResolveObject (ServiceCall call, ServiceBindParams parameters)
    {
        return Database.CluResolveAddress ("station", parameters.ObjectID);
    }

    protected override BoundService CreateBoundInstance (ServiceCall call, ServiceBindParams bindParams)
    {
        if (this.MachoResolveObject (call, bindParams) != BoundServiceManager.MachoNet.NodeID)
            throw new CustomError ("Trying to bind an object that does not belong to us!");

        Station station = this.Items.GetStaticStation (bindParams.ObjectID);

        if (station.HasService (Service.ReprocessingPlant) == false)
            throw new CustomError ("This station does not allow for reprocessing plant services");

        if (station.ID != call.Session.StationID)
            throw new CanOnlyDoInStations ();

        // load the inventory from dogma
        ItemInventory inventory = DogmaItems.LoadInventory (station.ID, call.Session.CharacterID);

        return new reprocessingSvc (
            ReprocessingDB, StandingDB, station, inventory, this.Items, BoundServiceManager, this.DogmaNotifications,
            call.Session, this.SolarSystems, DogmaItems
        );
    }
}