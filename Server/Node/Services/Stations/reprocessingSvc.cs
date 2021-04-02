﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Node.Database;
using Node.Exceptions;
using Node.Exceptions.jumpCloneSvc;
using Node.Exceptions.repairSvc;
using Node.Exceptions.reprocessingSvc;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Attributes;
using Node.Inventory.Items.Types;
using Node.Inventory.Notifications;
using Node.Market;
using Node.Network;
using Node.Services.Inventory;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Stations
{
    public class reprocessingSvc : BoundService
    {
        private static Dictionary<ItemTypes, ItemTypes> sOreTypeIDtoProcessingSkillTypeID = new Dictionary<ItemTypes, ItemTypes>()
        {
            {ItemTypes.Arkonor, ItemTypes.ArkonorProcessing},
            {ItemTypes.Bistot, ItemTypes.BistotProcessing},
            {ItemTypes.Crokite, ItemTypes.CrokiteProcessing},
            {ItemTypes.DarkOchre, ItemTypes.DarkOchreProcessing},
            {ItemTypes.Gneiss, ItemTypes.GneissProcessing},
            {ItemTypes.Hedbergite, ItemTypes.HedbergiteProcessing},
            {ItemTypes.Hemorphite, ItemTypes.HemorphiteProcessing},
            {ItemTypes.Jaspet, ItemTypes.JaspetProcessing},
            {ItemTypes.Kernite, ItemTypes.KerniteProcessing},
            {ItemTypes.Mercoxit, ItemTypes.MercoxitProcessing},
            {ItemTypes.Omber, ItemTypes.OmberProcessing},
            {ItemTypes.Plagioclase, ItemTypes.PlagioclaseProcessing},
            {ItemTypes.Pyroxeres, ItemTypes.PyroxeresProcessing},
            {ItemTypes.Scordite, ItemTypes.ScorditeProcessing},
            {ItemTypes.Spodumain, ItemTypes.SpodumainProcessing},
            {ItemTypes.Veldspar, ItemTypes.VeldsparProcessing},
        };
        
        private ItemFactory ItemFactory { get; }
        private ItemManager ItemManager => this.ItemFactory.ItemManager;
        private SystemManager SystemManager { get; }
        private TypeManager TypeManager => this.ItemFactory.TypeManager;
        private ItemInventory mInventory;
        private Station mStation;
        private Corporation mCorporation;
        private StandingDB StandingDB { get; }
        private ReprocessingDB ReprocessingDB { get; }

        public reprocessingSvc(ReprocessingDB reprocessingDb, StandingDB standingDb, ItemFactory itemFactory, SystemManager systemManager, BoundServiceManager manager) : base(manager, null)
        {
            this.ReprocessingDB = reprocessingDb;
            this.StandingDB = standingDb;
            this.ItemFactory = itemFactory;
            this.SystemManager = systemManager;
        }
        
        protected reprocessingSvc(ReprocessingDB reprocessingDb, StandingDB standingDb, Corporation corporation, Station station, ItemInventory inventory, ItemFactory itemFactory, SystemManager systemManager, BoundServiceManager manager, Client client) : base(manager, client)
        {
            this.ReprocessingDB = reprocessingDb;
            this.StandingDB = standingDb;
            this.mCorporation = corporation;
            this.mStation = station;
            this.mInventory = inventory;
            this.ItemFactory = itemFactory;
            this.SystemManager = systemManager;
        }

        public override PyInteger MachoResolveObject(PyInteger stationID, PyInteger zero, CallInformation call)
        {
            // TODO: CHECK IF THE GIVEN STATION HAS REPROCESSING SERVICES!
            
            if (this.SystemManager.StationBelongsToUs(stationID) == true)
                return this.BoundServiceManager.Container.NodeID;

            return this.SystemManager.GetNodeStationBelongsTo(stationID);
        }

        protected override BoundService CreateBoundInstance(PyDataType objectData, CallInformation call)
        {
            if (objectData is PyInteger == false)
                throw new CustomError("Cannot bind repairSvc service to unknown object");

            PyInteger stationID = objectData as PyInteger;
            
            if (this.MachoResolveObject(stationID, 0, call) != this.BoundServiceManager.Container.NodeID)
                throw new CustomError("Trying to bind an object that does not belong to us!");

            Station station = this.ItemManager.GetStaticStation(stationID);
            Corporation corporation = this.ItemManager.GetItem<Corporation>(station.OwnerID);
            ItemInventory inventory = this.ItemManager.MetaInventoryManager.RegisterMetaInventoryForOwnerID(station, call.Client.EnsureCharacterIsSelected());

            return new reprocessingSvc(this.ReprocessingDB, this.StandingDB, corporation, station, inventory, this.ItemFactory, this.SystemManager, this.BoundServiceManager, call.Client);
        }

        private double CalculateCombinedYield(Character character)
        {
            // there's no implants that affect the reprocessing of anything
            double efficiency = (0.375
                 * (1 + (0.02 * character.GetSkillLevel(ItemTypes.Refining)))
                 * (1 + (0.04 * character.GetSkillLevel(ItemTypes.RefineryEfficiency)))
            );

            efficiency += this.mStation.ReprocessingEfficiency;

            // efficiency should be maximum 1.0
            return Math.Min(efficiency, 1.0f);
        }

        private double CalculateEfficiency(Character character, int typeID)
        {
            // there's no implants that affect the reprocessing of anything
            double efficiency = (0.375
                 * (1 + (0.02 * character.GetSkillLevel(ItemTypes.Refining)))
                 * (1 + (0.04 * character.GetSkillLevel(ItemTypes.RefineryEfficiency)))
            );
            
            // check what mineral it is and calculate it's efficiency (there's skills that modify the outcome) 
            if (sOreTypeIDtoProcessingSkillTypeID.TryGetValue((ItemTypes) typeID, out ItemTypes skillType) == false)
                skillType = ItemTypes.ScrapmetalProcessing;

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
                standing += ((10.0 + standing) * 0.04 * character.GetSkillLevel(ItemTypes.Diplomacy));
            }
            else
            {
                standing += ((10.0 - standing) * 0.04 * character.GetSkillLevel(ItemTypes.Connections));
            }

            return standing;
        }

        public PyDataType GetReprocessingInfo(CallInformation call)
        {
            int stationID = call.Client.EnsureCharacterIsInStation();
            Character character = this.ItemManager.GetItem<Character>(call.Client.EnsureCharacterIsSelected());

            double standing = GetStanding(character);

            return new PyDictionary
            {
                ["yield"] = this.mStation.ReprocessingEfficiency,
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

            int leftovers = item.Quantity % item.Type.PortionSize;
            int quantityToProcess = item.Quantity - leftovers;
            
            double efficiency = this.CalculateEfficiency(character, item.Type.ID);

            List<ReprocessingDB.Recoverables> recoverablesList = this.ReprocessingDB.GetRecoverables(item.Type.ID);
            Rowset recoverables = new Rowset(
                new PyList(4)
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
                new PyList(4)
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
            Character character = this.ItemManager.GetItem<Character>(call.Client.EnsureCharacterIsSelected());
            
            PyDictionary<PyInteger, PyDataType> result = new PyDictionary<PyInteger, PyDataType>();

            foreach (PyInteger itemID in itemIDs.GetEnumerable<PyInteger>())
            {
                if (this.mInventory.Items.TryGetValue(itemID, out ItemEntity item) == false)
                    throw new MktNotOwner();

                result[itemID] = this.GetQuote(character, item);
            }
            
            return result;
        }

        public PyDataType Reprocess(PyList itemIDs, PyDataType ownerID, PyDataType flag, PyBool unknown, PyList skipChecks)
        {
            return null;
        }
    }
}