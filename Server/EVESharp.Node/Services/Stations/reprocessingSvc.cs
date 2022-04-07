using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Services;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Database;
using EVESharp.Node.Dogma;
using EVESharp.Node.Exceptions;
using EVESharp.Node.Exceptions.jumpCloneSvc;
using EVESharp.Node.Exceptions.reprocessingSvc;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Network;
using EVESharp.Node.Notifications.Client.Inventory;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.Node.Exceptions.repairSvc;
using EVESharp.Node.Inventory.Items.Attributes;
using EVESharp.Node.Market;
using EVESharp.Node.Services.Inventory;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;
using Service = EVESharp.Node.StaticData.Inventory.Station.Service;

namespace EVESharp.Node.Services.Stations;

public class reprocessingSvc : ClientBoundService
{
    public override AccessLevel AccessLevel => AccessLevel.None;
        
    private static Dictionary<Types, Types> sOreTypeIDtoProcessingSkillTypeID = new Dictionary<Types, Types>()
    {
        {Types.Arkonor, Types.ArkonorProcessing},
        {Types.Bistot, Types.BistotProcessing},
        {Types.Crokite, Types.CrokiteProcessing},
        {Types.DarkOchre, Types.DarkOchreProcessing},
        {Types.Gneiss, Types.GneissProcessing},
        {Types.Hedbergite, Types.HedbergiteProcessing},
        {Types.Hemorphite, Types.HemorphiteProcessing},
        {Types.Jaspet, Types.JaspetProcessing},
        {Types.Kernite, Types.KerniteProcessing},
        {Types.Mercoxit, Types.MercoxitProcessing},
        {Types.Omber, Types.OmberProcessing},
        {Types.Plagioclase, Types.PlagioclaseProcessing},
        {Types.Pyroxeres, Types.PyroxeresProcessing},
        {Types.Scordite, Types.ScorditeProcessing},
        {Types.Spodumain, Types.SpodumainProcessing},
        {Types.Veldspar, Types.VeldsparProcessing},
    };
        
    private ItemFactory      ItemFactory   { get; }
    private SystemManager    SystemManager => this.ItemFactory.SystemManager;
    private TypeManager      TypeManager   => this.ItemFactory.TypeManager;
    private ItemInventory    mInventory;
    private Station          mStation;
    private Corporation      mCorporation;
    private StandingDB       StandingDB     { get; }
    private ReprocessingDB   ReprocessingDB { get; }
    private Node.Dogma.Dogma Dogma          { get; }

    public reprocessingSvc(ReprocessingDB reprocessingDb, StandingDB standingDb, ItemFactory itemFactory, BoundServiceManager manager, Node.Dogma.Dogma dogma) : base(manager)
    {
        this.ReprocessingDB = reprocessingDb;
        this.StandingDB     = standingDb;
        this.ItemFactory    = itemFactory;
        this.Dogma          = dogma;
    }
        
    protected reprocessingSvc(ReprocessingDB reprocessingDb, StandingDB standingDb, Corporation corporation, Station station, ItemInventory inventory, ItemFactory itemFactory, BoundServiceManager manager, Node.Dogma.Dogma dogma, Session session) : base(manager, session, inventory.ID)
    {
        this.ReprocessingDB = reprocessingDb;
        this.StandingDB     = standingDb;
        this.mCorporation   = corporation;
        this.mStation       = station;
        this.mInventory     = inventory;
        this.ItemFactory    = itemFactory;
        this.Dogma          = dogma;
    }

    private double CalculateCombinedYield(Character character)
    {
        // there's no implants that affect the reprocessing of anything
        double efficiency = (0.375
                             * (1 + (0.02 * character.GetSkillLevel(Types.Refining)))
                             * (1 + (0.04 * character.GetSkillLevel(Types.RefineryEfficiency)))
            );

        efficiency += this.mStation.ReprocessingEfficiency;

        // efficiency should be maximum 1.0
        return Math.Min(efficiency, 1.0f);
    }

    private double CalculateEfficiency(Character character, int typeID)
    {
        // there's no implants that affect the reprocessing of anything
        double efficiency = (0.375
                             * (1 + (0.02 * character.GetSkillLevel(Types.Refining)))
                             * (1 + (0.04 * character.GetSkillLevel(Types.RefineryEfficiency)))
            );
            
        // check what mineral it is and calculate it's efficiency (there's skills that modify the outcome) 
        if (sOreTypeIDtoProcessingSkillTypeID.TryGetValue((Types) typeID, out Types skillType) == false)
            skillType = Types.ScrapmetalProcessing;

        // 5% increase by the specific metal skill
        efficiency *= (1 + (0.05 * character.GetSkillLevel(skillType)));
        // finally take into account station's efficienfy
        efficiency += this.mStation.ReprocessingEfficiency;

        // efficiency should be maximum 1.0
        return Math.Min(efficiency, 1.0f);
    }

    private double CalculateTax(double standing)
    {
        return Math.Max(0.0f, this.mCorporation.TaxRate - 0.75 / 100.0 * standing);
    }

    private double GetStanding(Character character)
    {
        // TODO: TAKE THIS ONE OUT OF HERE AND INTO THE PROPER PART OF THE SYSTEM
        double standing = this.StandingDB.GetStanding(this.mStation.OwnerID, character.ID);

        if (standing < 0.0f)
        {
            standing += ((10.0 + standing) * 0.04 * character.GetSkillLevel(Types.Diplomacy));
        }
        else
        {
            standing += ((10.0 - standing) * 0.04 * character.GetSkillLevel(Types.Connections));
        }

        return standing;
    }

    public PyDataType GetReprocessingInfo(CallInformation call)
    {
        int       stationID = call.Session.EnsureCharacterIsInStation();
        Character character = this.ItemFactory.GetItem<Character>(call.Session.EnsureCharacterIsSelected());

        double standing = GetStanding(character);

        return new PyDictionary
        {
            ["yield"]         = this.mStation.ReprocessingEfficiency,
            ["combinedyield"] = this.CalculateCombinedYield(character),
            ["wetake"] = new PyList(2)
            {
                [0] = CalculateTax(standing),
                [1] = standing
            },
        };
    }

    private PyDataType GetQuote(Character character, ItemEntity item)
    {
        if (item.Quantity < item.Type.PortionSize)
            throw new QuantityLessThanMinimumPortion(item.Type);

        int leftovers         = item.Quantity % item.Type.PortionSize;
        int quantityToProcess = item.Quantity - leftovers;
            
        List<ReprocessingDB.Recoverables> recoverablesList = this.ReprocessingDB.GetRecoverables(item.Type.ID);
        Rowset recoverables = new Rowset(
            new PyList<PyString>(4)
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

            double efficiency = this.CalculateEfficiency(character, recoverable.TypeID);
                
            recoverables.Rows.Add(
                new PyList(4)
                {
                    [0] = recoverable.TypeID,
                    [1] = (1.0 - efficiency) * ratio,
                    [2] = efficiency * this.mCorporation.TaxRate * ratio,
                    [3] = efficiency * (1.0 - this.mCorporation.TaxRate) * ratio
                }
            );
        }
            
        return new Row(
            new PyList<PyString>(4)
            {
                [0] = "leftOvers",
                [1] = "quantityToProcess",
                [2] = "playerStanding",
                [3] = "recoverables"
            },
            new PyList(4)
            {
                [0] = leftovers,
                [1] = quantityToProcess,
                [2] = GetStanding(character),
                [3] = recoverables
            }
        );
    }
        
    public PyDataType GetQuotes(PyList itemIDs, CallInformation call)
    {
        Character character = this.ItemFactory.GetItem<Character>(call.Session.EnsureCharacterIsSelected());
            
        PyDictionary<PyInteger, PyDataType> result = new PyDictionary<PyInteger, PyDataType>();

        foreach (PyInteger itemID in itemIDs.GetEnumerable<PyInteger>())
        {
            if (this.mInventory.Items.TryGetValue(itemID, out ItemEntity item) == false)
                throw new MktNotOwner();

            result[itemID] = this.GetQuote(character, item);
        }
            
        return result;
    }

    private void Reprocess(Character character, ItemEntity item, Session session)
    {
        if (item.Quantity < item.Type.PortionSize)
            throw new QuantityLessThanMinimumPortion(item.Type);
            
        int leftovers         = item.Quantity % item.Type.PortionSize;
        int quantityToProcess = item.Quantity - leftovers;
            
        List<ReprocessingDB.Recoverables> recoverablesList = this.ReprocessingDB.GetRecoverables(item.Type.ID);

        foreach (ReprocessingDB.Recoverables recoverable in recoverablesList)
        {
            int ratio = recoverable.AmountPerBatch * quantityToProcess / item.Type.PortionSize;

            double efficiency = this.CalculateEfficiency(character, recoverable.TypeID);

            int quantityForClient = (int) (efficiency * (1.0 - this.mCorporation.TaxRate) * ratio);
                
            // create the new item
            ItemEntity newItem = this.ItemFactory.CreateSimpleItem(this.TypeManager[recoverable.TypeID], character, this.mStation,
                                                                   Flags.Hangar, quantityForClient);
            // notify the client about the new item
            this.Dogma.QueueMultiEvent(session.EnsureCharacterIsSelected(), OnItemChange.BuildNewItemChange(newItem));
        }
    }

    public PyDataType Reprocess(PyList itemIDs, PyInteger ownerID, PyInteger flag, PyBool unknown, PyList skipChecks, CallInformation call)
    {
        Character character = this.ItemFactory.GetItem<Character>(call.Session.EnsureCharacterIsSelected());
            
        // TODO: TAKE INTO ACCOUNT OWNERID AND FLAG, THESE MOST LIKELY WILL BE USED BY CORP STUFF
        foreach (PyInteger itemID in itemIDs.GetEnumerable<PyInteger>())
        {
            if (this.mInventory.Items.TryGetValue(itemID, out ItemEntity item) == false)
                throw new MktNotOwner();

            // reprocess the item
            this.Reprocess(character, item, call.Session);
            int oldLocationID = item.LocationID;
            // finally remove the item from the inventories
            this.ItemFactory.DestroyItem(item);
            // notify the client about the item being destroyed
            this.Dogma.QueueMultiEvent(character.ID, OnItemChange.BuildLocationChange(item, oldLocationID));
        }
            
        return null;
    }

    protected override long MachoResolveObject(ServiceBindParams parameters, CallInformation call)
    {
        if (this.SystemManager.StationBelongsToUs(parameters.ObjectID) == true)
            return this.BoundServiceManager.MachoNet.NodeID;

        return this.SystemManager.GetNodeStationBelongsTo(parameters.ObjectID);
    }

    protected override BoundService CreateBoundInstance(ServiceBindParams bindParams, CallInformation call)
    {
        if (this.MachoResolveObject(bindParams, call) != this.BoundServiceManager.MachoNet.NodeID)
            throw new CustomError("Trying to bind an object that does not belong to us!");

        Station station = this.ItemFactory.GetStaticStation(bindParams.ObjectID);

        if (station.HasService(Service.ReprocessingPlant) == false)
            throw new CustomError("This station does not allow for reprocessing plant services");
        if (station.ID != call.Session.StationID)
            throw new CanOnlyDoInStations();

        Corporation corporation = this.ItemFactory.GetItem<Corporation>(station.OwnerID);
        ItemInventory inventory = this.ItemFactory.MetaInventoryManager.RegisterMetaInventoryForOwnerID(station, call.Session.EnsureCharacterIsSelected(), Flags.Hangar);
            
        return new reprocessingSvc(this.ReprocessingDB, this.StandingDB, corporation, station, inventory, this.ItemFactory, this.BoundServiceManager, this.Dogma, call.Session);
    }
}