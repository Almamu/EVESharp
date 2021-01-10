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
        private ItemFactory ItemFactory { get; }
        private Dictionary<int, ItemEntity> mItemList = new Dictionary<int, ItemEntity>();
        private Channel Log = null;
        private Dictionary<int, Station> mStations = new Dictionary<int, Station>();

        public Dictionary<int, Station> Stations => this.mStations;

        public void Load()
        {
            // create a log channel for the rare occurence of the ItemManager wanting to log something
            this.Log = this.ItemFactory.Container.Logger.CreateLogChannel("ItemManager");
            // load all the items in the database that do not belong to any user (so-called static items)
            List<ItemEntity> items = this.ItemFactory.ItemDB.LoadStaticItems();

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
                return this.PerformItemLoad(this.ItemFactory.ItemDB.LoadItem(itemID));
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
            }

            this.mItemList.Add(item.ID, item);

            return item;
        }

        public Dictionary<int, ItemEntity> LoadItemsLocatedAt(ItemEntity location)
        {
            return this.ItemFactory.ItemDB.LoadItemsLocatedAt(location.ID);
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
                    return this.ItemFactory.ItemDB.LoadSolarSystem(item);
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
            return this.ItemFactory.ItemDB.LoadBlueprint(item);
        }

        private ItemEntity LoadOwner(ItemEntity item)
        {
            switch (item.Type.Group.ID)
            {
                case (int) ItemGroups.Character:
                    return this.ItemFactory.ItemDB.LoadCharacter(item);
                case (int) ItemGroups.Corporation:
                    return this.ItemFactory.ItemDB.LoadCorporation(item);
                case (int) ItemGroups.Faction:
                    return this.ItemFactory.ItemDB.LoadFaction(item);
                default:
                    Log.Warning($"Loading owner {item.ID} from item group {item.Type.Group.ID} as normal item");
                    return item;
            }
        }

        private ItemEntity LoadSkill(ItemEntity item)
        {
            return this.ItemFactory.ItemDB.LoadSkill(item);
        }

        private ItemEntity LoadShip(ItemEntity item)
        {
            return this.ItemFactory.ItemDB.LoadShip(item);
        }

        private ItemEntity LoadStation(ItemEntity item)
        {
            Station station = this.ItemFactory.ItemDB.LoadStation(item);

            return this.mStations[station.ID] = station;
        }

        private ItemEntity LoadConstellation(ItemEntity item)
        {
            return this.ItemFactory.ItemDB.LoadConstellation(item);
        }

        private ItemEntity LoadRegion(ItemEntity item)
        {
            return this.ItemFactory.ItemDB.LoadRegion(item);
        }
        
        public ItemEntity CreateSimpleItem(string itemName, ItemType type, ItemEntity owner, ItemEntity location, ItemFlags flag,
            bool contraband, bool singleton, int quantity, double x, double y, double z, string customInfo)
        {
            int itemID = (int) this.ItemFactory.ItemDB.CreateItem(itemName, type, owner, location, flag, contraband, singleton, quantity, x, y, z, customInfo);

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
            int skillID = this.ItemFactory.SkillDB.CreateSkill(skillType, character);

            Skill skill = this.LoadItem(skillID) as Skill;

            // update skill level
            skill.Level = level;
            
            // add skill to the character's inventory
            character.AddItem(skill);

            this.ItemFactory.SkillDB.CreateSkillHistoryRecord(skillType, character,
                SkillHistoryReason.SkillTrainingComplete, skill.GetSkillPointsForLevel(level));

            return skill;
        }

        public Ship CreateShip(ItemType shipType, ItemEntity location, Character owner)
        {
            int shipID = (int) this.ItemFactory.ItemDB.CreateShip(shipType, location, owner);

            Ship ship = this.LoadItem(shipID) as Ship;

            return ship;
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
                this.ItemFactory.ItemDB.UnloadItem(item.ID);
            }
            catch
            {
            }
        }

        public ItemManager(ItemFactory factory)
        {
            this.ItemFactory = factory;
        }
    }
}