/*
    ------------------------------------------------------------------------------------
    LICENSE:
    ------------------------------------------------------------------------------------
    This file is part of EVE#: The EVE Online Server Emulator
    Copyright 2021 - EVE# Team
    ------------------------------------------------------------------------------------
    This program is free software; you can redistribute it and/or modify it under
    the terms of the GNU Lesser General Public License as published by the Free Software
    Foundation; either version 2 of the License, or (at your option) any later
    version.

    This program is distributed in the hope that it will be useful, but WITHOUT
    ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
    FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License along with
    this program; if not, write to the Free Software Foundation, Inc., 59 Temple
    Place - Suite 330, Boston, MA 02111-1307, USA, or go to
    http://www.gnu.org/copyleft/lesser.txt.
    ------------------------------------------------------------------------------------
    Creator: Almamu
*/

using System;
using System.Collections.Generic;
using Common.Logging;
using Node.Database;
using Node.Dogma;
using Node.Dogma.Interpreter;
using Node.Inventory.Exceptions;
using Node.Inventory.Items;
using Node.Inventory.Items.Dogma;
using Node.Inventory.Items.Types;
using Node.Inventory.SystemEntities;
using Node.StaticData;
using Node.StaticData.Inventory;
using SimpleInjector;
using Container = SimpleInjector.Container;
using Environment = Node.Dogma.Interpreter.Environment;
using Type = Node.StaticData.Inventory.Type;

namespace Node.Inventory
{
    public class ItemFactory
    {
        public const int FACTION_ID_MIN = 500000;
        public const int FACTION_ID_MAX = 1000000;
        public const int NPC_CORPORATION_ID_MIN = 1000000;
        public const int NPC_CORPORATION_ID_MAX = 2000000;
        public const int CELESTIAL_ID_MIN = 40000000;
        public const int CELESTIAL_ID_MAX = 50000000;
        public const int SOLARSYSTEM_ID_MIN = 30000000;
        public const int SOLARSYSTEM_ID_MAX = 40000000;
        public const int STATION_ID_MIN = 60000000;
        public const int STATION_ID_MAX = 70000000;
        public const int NPC_CHARACTER_ID_MIN = 10000;
        public const int NPC_CHARACTER_ID_MAX = 100000000;
        public const int USERGENERATED_ID_MIN = 100000000;
        
        public NodeContainer NodeContainer { get; }
        public AttributeManager AttributeManager { get; private set; }
        public ItemManager ItemManager { get; private set; }
        public CategoryManager CategoryManager { get; private set; }
        public GroupManager GroupManager { get; private set; }
        public TypeManager TypeManager { get; private set; }
        public StationManager StationManager { get; private set; }
        public SystemManager SystemManager { get; private set; }
        public ItemDB ItemDB { get; private set; }
        public CharacterDB CharacterDB { get; private set; }
        public InsuranceDB InsuranceDB { get; private set; }
        public SkillDB SkillDB { get; private set; }
        public Channel Log { get; init; }
        private Container DependencyInjection { get; }

        public MetaInventoryManager MetaInventoryManager { get; }
        private readonly Dictionary<int, ItemEntity> mItemList = new Dictionary<int, ItemEntity>();
        private readonly Dictionary<int, Station> mStations = new Dictionary<int, Station>();
        private readonly Dictionary<int, SolarSystem> mSolarSystems = new Dictionary<int, SolarSystem>();

        public Dictionary<int, Station> Stations => this.mStations;
        public Dictionary<int, SolarSystem> SolarSystems => this.mSolarSystems;
        public EVESystem LocationSystem { get; private set; }
        public EVESystem LocationRecycler { get; private set; }
        public EVESystem LocationMarket { get; private set; }
        public EVESystem LocationUniverse { get; private set; }
        public EVESystem LocationTemp { get; private set; }
        public ItemEntity SecureCommerceCommision { get; private set; }
        public ExpressionManager ExpressionManager { get; }

        public ItemFactory(Logger logger, NodeContainer nodeContainer, MetaInventoryManager metaInventoryManager, ExpressionManager expressionManager, Container dependencyInjection)
        {
            this.Log = logger.CreateLogChannel("ItemFactory");
            
            this.DependencyInjection = dependencyInjection;
            this.NodeContainer = nodeContainer;
            this.MetaInventoryManager = metaInventoryManager;
            this.ExpressionManager = expressionManager;
        }

        /// <summary>
        /// Initializes the item factory and loads the required subsystems
        /// </summary>
        public void Init()
        {
            this.ItemDB = this.DependencyInjection.GetInstance<ItemDB>();
            this.CharacterDB = this.DependencyInjection.GetInstance<CharacterDB>();
            this.InsuranceDB = this.DependencyInjection.GetInstance<InsuranceDB>();
            this.SkillDB = this.DependencyInjection.GetInstance<SkillDB>();

            this.SystemManager = this.DependencyInjection.GetInstance<SystemManager>();
            // station manager goes first
            this.StationManager = this.DependencyInjection.GetInstance<StationManager>();
            // attribute manager goes first
            this.AttributeManager = this.DependencyInjection.GetInstance<AttributeManager>();
            // category manager goes first
            this.CategoryManager = this.DependencyInjection.GetInstance<CategoryManager>();
            // then groups
            this.GroupManager = this.DependencyInjection.GetInstance<GroupManager>();
            // then the type manager
            this.TypeManager = this.DependencyInjection.GetInstance<TypeManager>();
            // finally the item manager
            this.ItemManager = this.DependencyInjection.GetInstance<ItemManager>();

            this.AttributeManager.Load();
            this.CategoryManager.Load();
            this.GroupManager.Load();
            this.TypeManager.Load();
            this.StationManager.Load();

            this.Load();
        }

        /// <summary>
        /// Initializes the item manager and loads the required items
        /// </summary>
        private void Load()
        {
            // load all the items in the database that do not belong to any user (so-called static items)
            List<ItemEntity> items = this.ItemDB.LoadStaticItems();

            foreach (ItemEntity item in items)
                this.PerformItemLoad(item);

            Log.Info($"Preloaded {this.mItemList.Count} static items");
            
            // store useful items like recycler and system
            this.LocationRecycler = this.GetItem<EVESystem>(this.NodeContainer.Constants[Constants.locationRecycler]);
            this.LocationSystem = this.GetItem<EVESystem>(this.NodeContainer.Constants[Constants.locationSystem]);
            this.LocationUniverse = this.GetItem<EVESystem>(this.NodeContainer.Constants[Constants.locationUniverse]);
            this.LocationMarket = this.GetItem<EVESystem>(this.NodeContainer.Constants[Constants.locationMarket]);
            this.LocationTemp = this.GetItem<EVESystem>(this.NodeContainer.Constants[Constants.locationTemp]);
            this.SecureCommerceCommision = this.GetItem(this.NodeContainer.Constants[Constants.ownerSecureCommerceCommission]);
        }

        /// <summary>
        /// Checks if an item exists and returns it
        /// </summary>
        /// <param name="itemID">The item to get</param>
        /// <param name="item">Where to put the item</param>
        /// <returns>Whether the item exists in the manager or not</returns>
        public bool TryGetItem(int itemID, out ItemEntity item)
        {
            return this.mItemList.TryGetValue(itemID, out item);
        }

        /// <summary>
        /// Shorthand method to get an item of an specific type
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool TryGetItem<T>(int itemID, out T item) where T : ItemEntity
        {
            item = null;
            
            if (this.mItemList.TryGetValue(itemID, out ItemEntity tmp) == false || tmp is T == false)
                return false;

            item = (T) tmp;
            
            return true;
        }

        /// <summary>
        /// Loads an item if it's not already loaded and returns it'
        /// </summary>
        /// <param name="itemID">The item to load</param>
        /// <returns>The loaded item</returns>
        public ItemEntity LoadItem(int itemID)
        {
            if (this.TryGetItem(itemID, out ItemEntity item) == false)
                return this.PerformItemLoad(this.ItemDB.LoadItem(itemID));

            return item;
        }

        /// <summary>
        /// Loads an item if it's not already loaded and returns it
        /// </summary>
        /// <param name="itemID">The item to load</param>
        /// <param name="loadRequired">Whether the item was loaded or already existed</param>
        /// <returns>The loaded item</returns>
        public ItemEntity LoadItem(int itemID, out bool loadRequired)
        {
            if (this.TryGetItem(itemID, out ItemEntity item) == false)
            {
                loadRequired = true;
                
                return this.PerformItemLoad(this.ItemDB.LoadItem(itemID));
            }

            loadRequired = false;

            return item;
        }
        
        /// <summary>
        /// Loads an item if it's not already loaded and returns it'
        /// </summary>
        /// <param name="itemID">The item to load</param>
        /// <returns>The loaded item</returns>
        public T LoadItem<T>(int itemID) where T : ItemEntity
        {
            return this.LoadItem(itemID) as T;
        }

        public T LoadItem<T>(int itemID, out bool loadRequired) where T : ItemEntity
        {
            return this.LoadItem(itemID, out loadRequired) as T;
        }

        /// <summary>
        /// Gets the given item
        /// </summary>
        /// <param name="itemID">The item to get</param>
        /// <returns>The item</returns>
        /// <exception cref="ItemNotLoadedException">If the item doesn't exist</exception>
        public ItemEntity GetItem(int itemID)
        {
            if (this.TryGetItem(itemID, out ItemEntity item) == false)
                throw new ItemNotLoadedException(itemID);

            return item;
        }

        public T GetItem<T>(int itemID) where T : ItemEntity
        {
            return this.GetItem(itemID) as T;
        }

        public static bool IsFactionID(int itemID)
        {
            return itemID >= FACTION_ID_MIN && itemID < FACTION_ID_MAX;
        }

        public static bool IsStationID(int itemID)
        {
            return itemID >= STATION_ID_MIN && itemID < STATION_ID_MAX;
        }

        public static bool IsSolarSystemID(int itemID)
        {
            return itemID >= SOLARSYSTEM_ID_MIN && itemID < SOLARSYSTEM_ID_MAX;
        }

        public static bool IsNPCCorporationID(int itemID)
        {
            return itemID >= NPC_CORPORATION_ID_MIN && itemID < NPC_CORPORATION_ID_MAX;
        }

        public static bool IsNPC(int itemID)
        {
            return itemID >= NPC_CHARACTER_ID_MIN && itemID < NPC_CHARACTER_ID_MAX;
        }

        public static bool IsCelestialID(int itemID)
        {
            return itemID >= CELESTIAL_ID_MIN && itemID < CELESTIAL_ID_MAX;
        }

        public static bool IsStaticData(int itemID)
        {
            return itemID < USERGENERATED_ID_MIN;
        }
        
        /// <summary>
        /// Gets the <see cref="ItemEntity"/> of the given faction
        /// </summary>
        /// <param name="factionID">The factionID to get</param>
        /// <returns>The item</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the id is not a faction</exception>
        public Faction GetStaticFaction(int factionID)
        {
            if (ItemFactory.IsFactionID(factionID) == false)
                throw new ArgumentOutOfRangeException($"The id {factionID} does not belong to a faction");

            return this.GetItem<Faction>(factionID);
        }
        
        /// <summary>
        /// Gets the <see cref="ItemEntity"/> of the given station
        /// </summary>
        /// <param name="stationID">The stationID to get</param>
        /// <returns>The item</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the id is not a station</exception>
        public Station GetStaticStation(int stationID)
        {
            if (ItemFactory.IsStationID(stationID) == false)
                throw new ArgumentOutOfRangeException($"The id {stationID} does not belong to a station");

            return this.GetItem<Station>(stationID);
        }
        
        /// <summary>
        /// Gets the <see cref="ItemEntity"/> of the given solar system
        /// </summary>
        /// <param name="solarSystemID">The solarSystemID to get</param>
        /// <returns>The item</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the id is not a solar system</exception>
        public SolarSystem GetStaticSolarSystem(int solarSystemID)
        {
            if (ItemFactory.IsSolarSystemID(solarSystemID) == false)
                throw new ArgumentOutOfRangeException($"The id {solarSystemID} does not belong to a solar system");
            
            return this.GetItem<SolarSystem>(solarSystemID);
        }

        /// <summary>
        /// Performs the extra, additional steps required for loading the given item
        /// </summary>
        /// <param name="item">The item to load</param>
        /// <returns>The new instance of the item with the extra information loaded</returns>
        private ItemEntity PerformItemLoad(ItemEntity item)
        {
            if (item is null)
                return null;

            item = item.Type.Group.Category.ID switch
            {
                // catch all for system items
                (int) Categories.System => LoadSystem(item),
                // celestial items are a kind of subcategory
                // load them in specific ways based on the type of celestial item
                (int) Categories.Celestial => LoadCelestial(item),
                (int) Categories.Blueprint => LoadBlueprint(item),
                // owner items are a kind of subcategory too
                (int) Categories.Owner => LoadOwner(item),
                (int) Categories.Skill => LoadSkill(item),
                (int) Categories.Ship => LoadShip(item),
                (int) Categories.Station => LoadStation(item),
                (int) Categories.Accessories => LoadAccessories(item),
                (int) Categories.Implant => LoadImplant(item),
                (int) Categories.Module => LoadModule(item),
                _ => item
            };

            // check if there's an inventory loaded that should contain this item
            if (this.mItemList.TryGetValue(item.LocationID, out ItemEntity location) == true && location is ItemInventory inventory)
                inventory.AddItem(item);
            
            // notify the meta inventory manager about the new item only if the item is user-generated
            if (item.ID >= USERGENERATED_ID_MIN)
                this.MetaInventoryManager.OnItemLoaded(item);

            // ensure the item is in the loaded list
            this.mItemList.Add(item.ID, item);

            // finally return the item
            return item;
        }

        public Dictionary<int, ItemEntity> LoadItemsLocatedAt(ItemEntity location, Flags ignoreFlag = Flags.None)
        {
            return this.ItemDB.LoadItemsLocatedAt(location.ID, ignoreFlag);
        }

        public Dictionary<int, ItemEntity> LoadItemsLocatedAtByOwner(ItemEntity location, int ownerID)
        {
            return this.ItemDB.LoadItemsLocatedAtByOwner(location.ID, ownerID);
        }

        public bool IsItemLoaded(int itemID)
        {
            return mItemList.ContainsKey(itemID);
        }

        private ItemEntity LoadSystem(ItemEntity item)
        {
            return new EVESystem(item);
        }

        private ItemEntity LoadCelestial(ItemEntity item)
        {
            switch (item.Type.Group.ID)
            {
                case (int) Groups.SolarSystem:
                    return this.LoadSolarSystem(item);
                case (int) Groups.Station:
                    return this.LoadStation(item);
                case (int) Groups.Constellation:
                    return this.LoadConstellation(item);
                case (int) Groups.Region:
                    return this.LoadRegion(item);
                case (int) Groups.CargoContainer:
                case (int) Groups.SecureCargoContainer:
                case (int) Groups.AuditLogSecureContainer:
                case (int) Groups.FreightContainer:
                case (int) Groups.Tool:
                    return this.LoadContainer(item);
                default:
                    Log.Warning($"Loading celestial {item.ID} from item group {item.Type.Group.ID} as normal item");
                    return item;
            }
        }
        
        private ItemEntity LoadBlueprint(ItemEntity item)
        {
            return this.ItemDB.LoadBlueprint(item);
        }

        private ItemEntity LoadOwner(ItemEntity item)
        {
            switch (item.Type.Group.ID)
            {
                case (int) Groups.Character:
                    return this.ItemDB.LoadCharacter(item);
                case (int) Groups.Corporation:
                    return this.ItemDB.LoadCorporation(item);
                case (int) Groups.Faction:
                    return this.ItemDB.LoadFaction(item);
                default:
                    Log.Warning($"Loading owner {item.ID} from item group {item.Type.Group.ID} as normal item");
                    return item;
            }
        }

        private ItemEntity LoadAccessories(ItemEntity item)
        {
            switch (item.Type.Group.ID)
            {
                case (int) Groups.Clone:
                    return this.ItemDB.LoadClone(item);
                default:
                    Log.Warning($"Loading accessory {item.ID} from item group {item.Type.Group.ID} as normal item");
                    return item;
            }
        }

        private ItemEntity LoadSkill(ItemEntity item)
        {
            return this.ItemDB.LoadSkill(item);
        }

        private ItemEntity LoadShip(ItemEntity item)
        {
            return this.ItemDB.LoadShip(item);
        }

        private Implant LoadImplant(ItemEntity item)
        {
            return this.ItemDB.LoadImplant(item);
        }

        private ItemEntity LoadSolarSystem(ItemEntity item)
        {
            SolarSystem solarSystem = this.ItemDB.LoadSolarSystem(item);

            return this.mSolarSystems[solarSystem.ID] = solarSystem;
        }

        private ItemEntity LoadStation(ItemEntity item)
        {
            Station station = this.ItemDB.LoadStation(item);

            return this.mStations[station.ID] = station;
        }

        private ItemEntity LoadConstellation(ItemEntity item)
        {
            return this.ItemDB.LoadConstellation(item);
        }

        private ItemEntity LoadRegion(ItemEntity item)
        {
            return this.ItemDB.LoadRegion(item);
        }

        private ItemEntity LoadContainer(ItemEntity item)
        {
            return new Items.Types.Container(item);
        }

        private ShipModule LoadModule(ItemEntity item)
        {
            return new ShipModule(item);
        }

        public ItemEntity CreateSimpleItem(Type type, int owner, int location, Flags flag, int quantity = 1,
            bool contraband = false, bool singleton = false)
        {
            return this.CreateSimpleItem(null, type.ID, owner, location, flag, quantity, contraband, singleton);
        }

        public ItemEntity CreateSimpleItem(string itemName, int typeID, int ownerID, int locationID, Flags flag,
            int quantity = 1, bool contraband = false, bool singleton = false, double x = 0.0, double y = 0.0, double z = 0.0,
            string customInfo = null)
        {
            int itemID = (int) this.ItemDB.CreateItem(itemName, typeID, ownerID, locationID, flag, contraband, singleton,
                quantity, x, y, z, customInfo);

            return this.LoadItem(itemID);
        }
        
        public ItemEntity CreateSimpleItem(string itemName, Type type, ItemEntity owner, ItemEntity location, Flags flag,
            bool contraband = false, bool singleton = false, int quantity = 1, double x = 0.0, double y = 0.0, double z = 0.0, string customInfo = null)
        {
            return this.CreateSimpleItem(itemName, type.ID, owner.ID, location.ID, flag, quantity, contraband, singleton,
                x, y, z, customInfo);
        }

        public ItemEntity CreateSimpleItem(Type type, ItemEntity owner, ItemEntity location, Flags flags,
            int quantity = 1, bool contraband = false, bool singleton = false)
        {
            return this.CreateSimpleItem(null, type, owner, location, flags, contraband, singleton, quantity);
        }

        public Skill CreateSkill(Type skillType, Character character, int level = 0, SkillHistoryReason reason = SkillHistoryReason.SkillTrainingComplete)
        {
            int skillID = this.SkillDB.CreateSkill(skillType, character);

            Skill skill = this.LoadItem<Skill>(skillID);

            // update skill level
            skill.Level = level;
            
            // add skill to the character's inventory
            character.AddItem(skill);

            // create a history entry if needed
            if (reason != SkillHistoryReason.None)
                this.SkillDB.CreateSkillHistoryRecord(skillType, character, reason, skill.GetSkillPointsForLevel(level));

            // persist the skill to the database
            skill.Persist();
            
            return skill;
        }

        public Ship CreateShip(Type shipType, ItemEntity location, Character owner)
        {
            int shipID = (int) this.ItemDB.CreateShip(shipType, location, owner);

            Ship ship = this.LoadItem<Ship>(shipID);

            return ship;
        }

        public Clone CreateClone(Type cloneType, ItemEntity location, Character owner)
        {
            return this.CreateSimpleItem(cloneType, owner, location, Flags.Clone, 1, false, true) as Clone;
        }

        public void UnloadItem(ItemEntity item)
        {
            // first ensure there's no meta inventory holding this item hostage
            if (this.MetaInventoryManager.GetOwnerInventoryAtLocation(item.LocationID, item.OwnerID, out ItemInventoryByOwnerID _) == true)
                return;
            
            if (this.mItemList.Remove(item.ID) == false)
                return;

            // dispose of it
            item.Dispose();

            // update the ownership information
            this.ItemDB.UnloadItem(item.ID);
        }

        public void UnloadItem(int itemID)
        {
            if (this.TryGetItem(itemID, out ItemEntity item) == false)
                return;
            
            this.UnloadItem(item);
        }

        public void DestroyItem(ItemEntity item)
        {
            // remove the item off the list and throw an exception if the item wasn't loaded in this item manager
            if (this.mItemList.Remove(item.ID) == false)
                throw new ArgumentException("Cannot destroy an item that was not loaded by this item manager");

            // ensure the meta inventories know this item is not there anymore
            this.MetaInventoryManager.OnItemDestroyed(item);
            
            // make sure the location it's at knows the item is no more
            if (this.TryGetItem(item.LocationID, out ItemEntity location) == true && location is ItemInventory inventory)
                inventory.RemoveItem(item);
            
            // set the item to the recycler location just in case something has a reference to it somewhere
            item.LocationID = this.LocationRecycler.ID;
            // item.Flag = ItemFlags.None;

            // finally remove the item off the database
            item.Destroy();
        }

        public void DestroyItems(Dictionary<int, ItemEntity> items)
        {
            foreach (KeyValuePair<int, ItemEntity> pair in items)
                this.DestroyItem(pair.Value);
        }

        public void DestroyItems(List<ItemEntity> items)
        {
            foreach (ItemEntity item in items)
                this.DestroyItem(item);
        }
        
    }
}