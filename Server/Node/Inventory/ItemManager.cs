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
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using Node.Inventory.SystemEntities;

namespace Node.Inventory
{
    public class ItemManager
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

        private SkillDB SkillDB { get; }
        private ItemDB ItemDB { get; }
        private Channel Log { get; }
        private NodeContainer NodeContainer { get; }
        public MetaInventoryManager MetaInventoryManager { get; }
        private Dictionary<int, ItemEntity> mItemList = new Dictionary<int, ItemEntity>();
        private Dictionary<int, Station> mStations = new Dictionary<int, Station>();
        private Dictionary<int, SolarSystem> mSolarSystems = new Dictionary<int, SolarSystem>();

        public Dictionary<int, Station> Stations => this.mStations;
        public Dictionary<int, SolarSystem> SolarSystems => this.mSolarSystems;
        public ItemEntity LocationSystem { get; private set; }
        public ItemEntity LocationRecycler { get; private set; }
        public ItemEntity SecureCommerceCommision { get; private set; }

        public void Load()
        {
            // load all the items in the database that do not belong to any user (so-called static items)
            List<ItemEntity> items = this.ItemDB.LoadStaticItems();

            foreach (ItemEntity item in items)
            {
                this.PerformItemLoad(item);
            }

            Log.Info($"Preloaded {this.mItemList.Count} static items");
            
            // store useful items like recycler and system
            this.LocationRecycler = this.GetItem(this.NodeContainer.Constants["locationRecycler"]);
            this.LocationSystem = this.GetItem(this.NodeContainer.Constants["locationSystem"]);
            this.SecureCommerceCommision = this.GetItem(this.NodeContainer.Constants["ownerSecureCommerceCommission"]);
        }

        public ItemEntity LoadItem(int itemID)
        {
            if (IsItemLoaded(itemID) == false)
            {
                return this.PerformItemLoad(this.ItemDB.LoadItem(itemID));
            }
            else
            {
                return this.mItemList[itemID];
            }
        }

        public ItemEntity GetItem(int itemID)
        {
            return this.mItemList[itemID];
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
        
        public Faction GetFaction(int factionID)
        {
            if (ItemManager.IsFactionID(factionID) == false)
                throw new ArgumentOutOfRangeException($"The id {factionID} does not belong to a faction");

            return this.GetItem(factionID) as Faction;
        }

        public Station GetStation(int stationID)
        {
            if (ItemManager.IsStationID(stationID) == false)
                throw new ArgumentOutOfRangeException($"The id {stationID} does not belong to a station");

            return this.GetItem(stationID) as Station;
        }

        public SolarSystem GetSolarSystem(int solarSystemID)
        {
            if (ItemManager.IsSolarSystemID(solarSystemID) == false)
                throw new ArgumentOutOfRangeException($"The id {solarSystemID} does not belong to a solar system");
            
            return this.GetItem(solarSystemID) as SolarSystem;
        }

        private ItemEntity PerformItemLoad(ItemEntity item)
        {
            if (item == null)
                return null;

            switch (item.Type.Group.Category.ID)
            {
                // celestial items are a kind of subcategory
                // load them in specific ways based on the type of celestial item
                case (int) ItemCategories.Celestial:
                    item = LoadCelestial(item);
                    break;

                case (int) ItemCategories.Blueprint:
                    item = LoadBlueprint(item);
                    break;

                // owner items are a kind of subcategory too
                case (int) ItemCategories.Owner:
                    item = LoadOwner(item);
                    break;
                    
                case (int) ItemCategories.Skill:
                    item = LoadSkill(item);
                    break;
                    
                case (int) ItemCategories.Ship:
                    item = LoadShip(item);
                    break;
                    
                case (int) ItemCategories.Station:
                    item = LoadStation(item);
                    break;
                
                case (int) ItemCategories.Accessories:
                    item = LoadAccessories(item);
                    break;
            }

            this.mItemList.Add(item.ID, item);

            return item;
        }

        public Dictionary<int, ItemEntity> LoadItemsLocatedAt(ItemEntity location, ItemFlags ignoreFlag = ItemFlags.None)
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

        private ItemEntity LoadCelestial(ItemEntity item)
        {
            switch (item.Type.Group.ID)
            {
                case (int) ItemGroups.SolarSystem:
                    return this.LoadSolarSystem(item);
                case (int) ItemGroups.Station:
                    return this.LoadStation(item);
                case (int) ItemGroups.Constellation:
                    return this.LoadConstellation(item);
                case (int) ItemGroups.Region:
                    return this.LoadRegion(item);
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
                case (int) ItemGroups.Character:
                    return this.ItemDB.LoadCharacter(item);
                case (int) ItemGroups.Corporation:
                    return this.ItemDB.LoadCorporation(item);
                case (int) ItemGroups.Faction:
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
                case (int) ItemGroups.Clone:
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

        public ItemEntity CreateSimpleItem(ItemType type, int owner, int location, ItemFlags flag, int quantity = 1,
            bool contraband = false, bool singleton = false)
        {
            return this.CreateSimpleItem(type.Name, type.ID, owner, location, flag, quantity, contraband, singleton);
        }

        public ItemEntity CreateSimpleItem(string itemName, int typeID, int ownerID, int locationID, ItemFlags flag,
            int quantity = 1, bool contraband = false, bool singleton = false, double x = 0.0, double y = 0.0, double z = 0.0,
            string customInfo = null)
        {
            int itemID = (int) this.ItemDB.CreateItem(itemName, typeID, ownerID, locationID, flag, contraband, singleton,
                quantity, 0, 0, 0, null);

            ItemEntity entity = this.LoadItem(itemID);
            
            // check if the inventory that loads this item is loaded
            // and ensure the item is added in there
            if (this.IsItemLoaded(entity.LocationID) == true)
            {
                ItemInventory inventory = this.GetItem(entity.LocationID) as ItemInventory;

                inventory.AddItem(entity);
                
                // look for any metainventories too
                try
                {
                    ItemInventory metaInventory =
                        this.MetaInventoryManager.GetOwnerInventoriesAtLocation(entity.LocationID, entity.OwnerID);

                    metaInventory.AddItem(entity);
                }
                catch (ArgumentOutOfRangeException)
                {
                    // ignore the exception, this is expected when no meta inventories are registered
                }
            }

            return entity;
        }
        
        public ItemEntity CreateSimpleItem(string itemName, ItemType type, ItemEntity owner, ItemEntity location, ItemFlags flag,
            bool contraband = false, bool singleton = false, int quantity = 1, double x = 0.0, double y = 0.0, double z = 0.0, string customInfo = null)
        {
            return this.CreateSimpleItem(itemName, type.ID, owner.ID, location.ID, flag, quantity, contraband, singleton,
                x, y, z, customInfo);
        }

        public ItemEntity CreateSimpleItem(ItemType type, ItemEntity owner, ItemEntity location, ItemFlags flags,
            int quantity = 1, bool contraband = false, bool singleton = false)
        {
            return this.CreateSimpleItem(type.Name, type, owner, location, flags, contraband, singleton, quantity, 0, 0,
                0, null);
        }

        public Skill CreateSkill(ItemType skillType, Character character, int level = 0, SkillHistoryReason reason = SkillHistoryReason.SkillTrainingComplete)
        {
            int skillID = this.SkillDB.CreateSkill(skillType, character);

            Skill skill = this.LoadItem(skillID) as Skill;

            // update skill level
            skill.Level = level;
            
            // add skill to the character's inventory
            character.AddItem(skill);

            // create a history entry if needed
            if (reason != SkillHistoryReason.None)
                this.SkillDB.CreateSkillHistoryRecord(skillType, character, reason, skill.GetSkillPointsForLevel(level));

            return skill;
        }

        public Ship CreateShip(ItemType shipType, ItemEntity location, Character owner)
        {
            int shipID = (int) this.ItemDB.CreateShip(shipType, location, owner);

            Ship ship = this.LoadItem(shipID) as Ship;

            return ship;
        }

        public Clone CreateClone(ItemType cloneType, ItemEntity location, Character owner)
        {
            return this.CreateSimpleItem(cloneType, owner, location, ItemFlags.Clone, 1, false, true) as Clone;
        }

        public void UnloadItem(ItemEntity item)
        {
            try
            {
                mItemList.Remove(item.ID);

                // Update the database informationi
                this.ItemDB.UnloadItem(item.ID);
            }
            catch
            {
                // ignored
            }
        }

        public void UnloadItem(int itemID)
        {
            try
            {
                this.mItemList.Remove(itemID);
                this.ItemDB.UnloadItem(itemID);
            }
            catch
            {
                // ignored
            }
        }

        public void DestroyItem(ItemEntity item)
        {
            if (this.IsItemLoaded(item.ID) == false)
                throw new ArgumentException("Cannot destroy an item that was not loaded by this item manager");

            // remove the item from the list
            this.mItemList.Remove(item.ID);
            
            // check if there are inventories loaded for this item
            if (this.IsItemLoaded(item.LocationID) == true)
            {
                ItemInventory inventory = this.GetItem(item.LocationID) as ItemInventory;
                
                // remove the item from the inventory
                inventory.RemoveItem(item);
                
                // try to remove from meta inventories too
                try
                {
                    ItemInventory metaInventory =
                        this.MetaInventoryManager.GetOwnerInventoriesAtLocation(item.LocationID, item.OwnerID);

                    metaInventory.RemoveItem(item);
                }
                catch (ArgumentOutOfRangeException)
                {
                }
            }

            // set the item to the recycler location
            item.LocationID = this.LocationRecycler.ID;

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
        
        public ItemManager(Logger logger, ItemDB itemDB, SkillDB skillDB, NodeContainer nodeContainer, MetaInventoryManager metaInventoryManager)
        {
            // create a log channel for the rare occurence of the ItemManager wanting to log something
            this.Log = logger.CreateLogChannel("ItemManager");
            this.ItemDB = itemDB;
            this.SkillDB = skillDB;
            this.NodeContainer = nodeContainer;
            this.MetaInventoryManager = metaInventoryManager;
        }
    }
}