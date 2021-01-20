/*
    ------------------------------------------------------------------------------------
    LICENSE:
    ------------------------------------------------------------------------------------
    This file is part of EVE#: The EVE Online Server Emulator
    Copyright 2012 - Glint Development Group
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
using Common.Database;
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
        public const int STATION_ID_MIN = 60000000;
        public const int STATION_ID_MAX = 70000000;
        public const int NPC_CHARACTER_ID_MIN = 10000;
        public const int NPC_CHARACTER_ID_MAX = 100000000;
        public const int USERGENERATED_ID_MIN = 100000000;

        private SkillDB SkillDB { get; }
        private ItemDB ItemDB { get; }
        private Channel Log { get; }
        private Dictionary<int, ItemEntity> mItemList = new Dictionary<int, ItemEntity>();
        private Dictionary<int, Station> mStations = new Dictionary<int, Station>();

        public Dictionary<int, Station> Stations => this.mStations;

        public void Load()
        {
            // load all the items in the database that do not belong to any user (so-called static items)
            List<ItemEntity> items = this.ItemDB.LoadStaticItems();

            foreach (ItemEntity item in items)
            {
                this.PerformItemLoad(item);
            }

            Log.Info($"Preloaded {this.mItemList.Count} static items");
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

        public static bool IsNPCCorporationID(int itemID)
        {
            return itemID >= NPC_CORPORATION_ID_MIN && itemID < NPC_CORPORATION_ID_MAX;
        }

        public static bool IsNPC(int itemID)
        {
            return itemID >= NPC_CHARACTER_ID_MIN && itemID < NPC_CHARACTER_ID_MAX;
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
                throw new ArgumentOutOfRangeException($"The id {stationID}does not belong to a station");

            return this.GetItem(stationID) as Station;
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

        public Dictionary<int, ItemEntity> LoadItemsLocatedAt(ItemEntity location)
        {
            return this.ItemDB.LoadItemsLocatedAt(location.ID);
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
                    return this.ItemDB.LoadSolarSystem(item);
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
        
        public ItemEntity CreateSimpleItem(string itemName, ItemType type, ItemEntity owner, ItemEntity location, ItemFlags flag,
            bool contraband, bool singleton, int quantity, double x, double y, double z, string customInfo)
        {
            int itemID = (int) this.ItemDB.CreateItem(itemName, type, owner, location, flag, contraband, singleton, quantity, x, y, z, customInfo);

            return this.LoadItem(itemID);
        }

        public ItemEntity CreateSimpleItem(ItemType type, ItemEntity owner, ItemEntity location, ItemFlags flags,
            int quantity = 1, bool contraband = false, bool singleton = false)
        {
            return this.CreateSimpleItem(type.Name, type, owner, location, flags, contraband, singleton, quantity, 0, 0,
                0, null);
        }

        public Skill CreateSkill(ItemType skillType, Character character, int level = 0)
        {
            int skillID = this.SkillDB.CreateSkill(skillType, character);

            Skill skill = this.LoadItem(skillID) as Skill;

            // update skill level
            skill.Level = level;
            
            // add skill to the character's inventory
            character.AddItem(skill);

            this.SkillDB.CreateSkillHistoryRecord(skillType, character,
                SkillHistoryReason.SkillTrainingComplete, skill.GetSkillPointsForLevel(level));

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

        public void MoveItem(ItemEntity item, ItemEntity newLocation)
        {
            // TODO: SEND NOTIFICATION OF ITEM CHANGE?
            item.LocationID = newLocation.ID;
        }

        public void ChangeOwnership(ItemEntity item, ItemEntity newOwner)
        {
            // TODO: SEND NOTIFICATION OF ITEM CHANGE?
            item.OwnerID = newOwner.ID;
        }

        public void UnloadItem(ItemEntity item)
        {
            try
            {
                mItemList.Remove(item.ID);

                // Update the database information
                this.ItemDB.UnloadItem(item.ID);
            }
            catch
            {
            }
        }

        public void DestroyItem(ItemEntity item)
        {
            if (this.IsItemLoaded(item.ID) == false)
                throw new ArgumentException("Cannot destroy an item that was not loaded by this item manager");

            // remove the item from the list
            this.mItemList.Remove(item.ID);

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
        
        public ItemManager(Logger logger, ItemDB itemDB, SkillDB skillDB)
        {
            // create a log channel for the rare occurence of the ItemManager wanting to log something
            this.Log = logger.CreateLogChannel("ItemManager");
            this.ItemDB = itemDB;
            this.SkillDB = skillDB;
        }
    }
}