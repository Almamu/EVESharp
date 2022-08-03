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
using System.Collections.Concurrent;
using System.Collections.Generic;
using EVESharp.Database;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.Node.Configuration;
using EVESharp.Node.Database;
using EVESharp.Node.Dogma;
using EVESharp.Node.Inventory.Exceptions;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Server.Shared;
using EVESharp.PythonTypes.Types.Database;
using Serilog;
using Character = EVESharp.Node.Inventory.Items.Types.Character;
using Container = SimpleInjector.Container;
using Item = EVESharp.Node.Inventory.Items.Types.Information.Item;
using ItemDB = EVESharp.Node.Database.ItemDB;
using Type = EVESharp.EVE.StaticData.Inventory.Type;

namespace EVESharp.Node.Inventory;

public class ItemFactory
{
    private readonly Dictionary <int, ItemEntity> mItemList = new Dictionary <int, ItemEntity> ();

    public  AttributeManager AttributeManager    { get; private set; }
    public  Categories       Categories          { get; private set; }
    public  Groups           Groups              { get; private set; }
    public  TypeManager      TypeManager         { get; private set; }
    public  StationManager   StationManager      { get; private set; }
    public  SystemManager    SystemManager       { get; private set; }
    public  Ancestries       Ancestries          { get; private set; }
    public  Bloodlines       Bloodlines          { get; private set; }
    public  DogmaUtils       DogmaUtils          { get; private set; }
    public  ItemDB           ItemDB              { get; private set; }
    public  CharacterDB      CharacterDB         { get; private set; }
    public  CorporationDB    CorporationDB       { get; private set; }
    public  InsuranceDB      InsuranceDB         { get; private set; }
    public  SkillDB          SkillDB             { get; private set; }
    public  ILogger          Log                 { get; init; }
    public  Constants        Constants           { get; }
    public  IMachoNet        MachoNet            { get; }
    private Container        DependencyInjection { get; }

    public MetaInventoryManager MetaInventoryManager { get; }

    public Dictionary <int, Station>     Stations          { get; } = new Dictionary <int, Station> ();
    public Dictionary <int, SolarSystem> SolarSystems      { get; } = new Dictionary <int, SolarSystem> ();
    public Dictionary <int, Faction>     Factions          { get; } = new Dictionary <int, Faction> ();
    public EVESystem                     OwnerBank         { get; private set; }
    public EVESystem                     LocationSystem    { get; private set; }
    public EVESystem                     LocationRecycler  { get; private set; }
    public EVESystem                     LocationMarket    { get; private set; }
    public EVESystem                     LocationUniverse  { get; private set; }
    public EVESystem                     LocationTemp      { get; private set; }
    public ItemEntity                    OwnerSCC          { get; private set; }
    public ExpressionManager             ExpressionManager { get; }

    protected IDatabaseConnection Database { get; }

    public ItemFactory (
        ILogger           logger, IMachoNet machoNet, IDatabaseConnection databaseConnection, Constants constants, MetaInventoryManager metaInventoryManager,
        ExpressionManager expressionManager, Container dependencyInjection
    )
    {
        Log = logger;

        Database                                    =  databaseConnection;
        DependencyInjection                         =  dependencyInjection;
        MachoNet                                    =  machoNet;
        Constants                                   =  constants;
        MetaInventoryManager                        =  metaInventoryManager;
        ExpressionManager                           =  expressionManager;
        MetaInventoryManager.OnMetaInventoryCreated += this.OnMetaInventoryCreated;
    }

    /// <summary>
    /// Initializes the item factory and loads the required subsystems
    /// </summary>
    public void Init ()
    {
        ItemDB        = DependencyInjection.GetInstance <ItemDB> ();
        CharacterDB   = DependencyInjection.GetInstance <CharacterDB> ();
        InsuranceDB   = DependencyInjection.GetInstance <InsuranceDB> ();
        SkillDB       = DependencyInjection.GetInstance <SkillDB> ();
        CorporationDB = DependencyInjection.GetInstance <CorporationDB> ();

        SystemManager = DependencyInjection.GetInstance <SystemManager> ();
        // station manager goes first
        StationManager = DependencyInjection.GetInstance <StationManager> ();
        // attribute manager goes first
        AttributeManager = DependencyInjection.GetInstance <AttributeManager> ();
        // category manager goes first
        Categories = DependencyInjection.GetInstance <Categories> ();
        // then groups
        Groups = DependencyInjection.GetInstance <Groups> ();
        // then the type manager
        TypeManager = DependencyInjection.GetInstance <TypeManager> ();
        // bloodlines are required too
        Bloodlines = DependencyInjection.GetInstance <Bloodlines> ();
        // the ancestry manager is also needed
        Ancestries = DependencyInjection.GetInstance <Ancestries> ();
        DogmaUtils = DependencyInjection.GetInstance <DogmaUtils> ();

        AttributeManager.Load ();
        Categories.Load ();
        Groups.Load ();
        TypeManager.Load ();
        StationManager.Load ();
        Bloodlines.Load ();
        Ancestries.Load ();

        this.Load ();
    }

    /// <summary>
    /// Initializes the item manager and loads the required items
    /// </summary>
    private void Load ()
    {
        // load all the items in the database that do not belong to any user (so-called static items)
        foreach (Item item in ItemDB.LoadStaticItems ())
            this.PerformItemLoad (item);

        Log.Information ($"Preloaded {this.mItemList.Count} static items");

        // store useful items like recycler and system
        LocationRecycler = this.GetItem <EVESystem> (Constants.LocationRecycler);
        LocationSystem   = this.GetItem <EVESystem> (Constants.LocationSystem);
        LocationUniverse = this.GetItem <EVESystem> (Constants.LocationUniverse);
        LocationMarket   = this.GetItem <EVESystem> (Constants.LocationMarket);
        LocationTemp     = this.GetItem <EVESystem> (Constants.LocationTemp);
        OwnerBank        = this.GetItem <EVESystem> (Constants.OwnerBank);
        OwnerSCC         = this.GetItem (Constants.OwnerSecureCommerceCommission);
    }

    /// <summary>
    /// Checks if an item exists and returns it
    /// </summary>
    /// <param name="itemID">The item to get</param>
    /// <param name="item">Where to put the item</param>
    /// <returns>Whether the item exists in the manager or not</returns>
    public bool TryGetItem (int itemID, out ItemEntity item)
    {
        lock (this)
        {
            item = null;

            return this.mItemList.TryGetValue (itemID, out item);
        }
    }

    /// <summary>
    /// Shorthand method to get an item of an specific type
    /// </summary>
    /// <param name="itemID"></param>
    /// <param name="item"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool TryGetItem <T> (int itemID, out T item) where T : ItemEntity
    {
        lock (this)
        {
            item = null;

            if (this.mItemList.TryGetValue (itemID, out ItemEntity tmp) == false || tmp is not T)
                return false;

            item = (T) tmp;

            return true;
        }
    }

    /// <summary>
    /// Loads an item if it's not already loaded and returns it'
    /// </summary>
    /// <param name="itemID">The item to load</param>
    /// <returns>The loaded item</returns>
    public ItemEntity LoadItem (int itemID)
    {
        lock (this)
        {
            if (this.TryGetItem (itemID, out ItemEntity item) == false)
                return this.PerformItemLoad (ItemDB.LoadItem (itemID, MachoNet.NodeID));

            return item;
        }
    }

    /// <summary>
    /// Loads an item if it's not already loaded and returns it
    /// </summary>
    /// <param name="itemID">The item to load</param>
    /// <param name="loadRequired">Whether the item was loaded or already existed</param>
    /// <returns>The loaded item</returns>
    public ItemEntity LoadItem (int itemID, out bool loadRequired)
    {
        lock (this)
        {
            if (this.TryGetItem (itemID, out ItemEntity item) == false)
            {
                loadRequired = true;

                return this.PerformItemLoad (ItemDB.LoadItem (itemID, MachoNet.NodeID));
            }

            loadRequired = false;

            return item;
        }
    }

    /// <summary>
    /// Loads an item if it's not already loaded and returns it'
    /// </summary>
    /// <param name="itemID">The item to load</param>
    /// <returns>The loaded item</returns>
    public T LoadItem <T> (int itemID) where T : ItemEntity
    {
        return this.LoadItem (itemID) as T;
    }

    public T LoadItem <T> (int itemID, out bool loadRequired) where T : ItemEntity
    {
        return this.LoadItem (itemID, out loadRequired) as T;
    }

    /// <summary>
    /// Gets the given item
    /// </summary>
    /// <param name="itemID">The item to get</param>
    /// <returns>The item</returns>
    /// <exception cref="ItemNotLoadedException">If the item doesn't exist</exception>
    public ItemEntity GetItem (int itemID)
    {
        lock (this)
        {
            if (this.TryGetItem (itemID, out ItemEntity item) == false)
                throw new ItemNotLoadedException (itemID);

            return item;
        }
    }

    public T GetItem <T> (int itemID) where T : ItemEntity
    {
        return this.GetItem (itemID) as T;
    }

    /// <summary>
    /// Gets the <see cref="ItemEntity"/> of the given faction
    /// </summary>
    /// <param name="factionID">The factionID to get</param>
    /// <returns>The item</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the id is not a faction</exception>
    public Faction GetStaticFaction (int factionID)
    {
        if (ItemRanges.IsFactionID (factionID) == false)
            throw new ArgumentOutOfRangeException ($"The id {factionID} does not belong to a faction");

        return this.GetItem <Faction> (factionID);
    }

    /// <summary>
    /// Gets the <see cref="ItemEntity"/> of the given station
    /// </summary>
    /// <param name="stationID">The stationID to get</param>
    /// <returns>The item</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the id is not a station</exception>
    public Station GetStaticStation (int stationID)
    {
        if (ItemRanges.IsStationID (stationID) == false)
            throw new ArgumentOutOfRangeException ($"The id {stationID} does not belong to a station");

        return this.GetItem <Station> (stationID);
    }

    /// <summary>
    /// Gets the <see cref="ItemEntity"/> of the given solar system
    /// </summary>
    /// <param name="solarSystemID">The solarSystemID to get</param>
    /// <returns>The item</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the id is not a solar system</exception>
    public SolarSystem GetStaticSolarSystem (int solarSystemID)
    {
        if (ItemRanges.IsSolarSystemID (solarSystemID) == false)
            throw new ArgumentOutOfRangeException ($"The id {solarSystemID} does not belong to a solar system");

        return this.GetItem <SolarSystem> (solarSystemID);
    }

    /// <summary>
    /// Performs the extra, additional steps required for loading the given item
    /// </summary>
    /// <param name="item">The item to load</param>
    /// <returns>The new instance of the item with the extra information loaded</returns>
    private ItemEntity PerformItemLoad (Item item)
    {
        lock (this)
        {
            if (item is null)
                return null;

            ItemEntity wrapperItem = item.Type.Group.Category.ID switch
            {
                // catch all for system items
                (int) EVE.StaticData.Inventory.Categories.System => this.LoadSystem (item),
                // celestial items are a kind of subcategory
                // load them in specific ways based on the type of celestial item
                (int) EVE.StaticData.Inventory.Categories.Celestial => this.LoadCelestial (item),
                (int) EVE.StaticData.Inventory.Categories.Blueprint => this.LoadBlueprint (item),
                // owner items are a kind of subcategory too
                (int) EVE.StaticData.Inventory.Categories.Owner       => this.LoadOwner (item),
                (int) EVE.StaticData.Inventory.Categories.Skill       => this.LoadSkill (item),
                (int) EVE.StaticData.Inventory.Categories.Ship        => this.LoadShip (item),
                (int) EVE.StaticData.Inventory.Categories.Station     => this.LoadStation (item),
                (int) EVE.StaticData.Inventory.Categories.Accessories => this.LoadAccessories (item),
                (int) EVE.StaticData.Inventory.Categories.Implant     => this.LoadImplant (item),
                (int) EVE.StaticData.Inventory.Categories.Module      => this.LoadModule (item),
                _                                                     => new Items.Types.Item (item)
            };

            // check if there's an inventory loaded that should contain this item
            if (this.TryGetItem (item.LocationID, out ItemEntity location) && location is ItemInventory inventory)
                inventory.AddItem (wrapperItem);

            // notify the meta inventory manager about the new item only if the item is user-generated
            if (item.ID >= ItemRanges.USERGENERATED_ID_MIN)
                MetaInventoryManager.OnItemLoaded (wrapperItem);

            // ensure the item is in the loaded list
            this.mItemList.Add (item.ID, wrapperItem);

            // subscribe to item's events and return the item
            return this.SubscribeToEvents (wrapperItem);
        }
    }

    public ConcurrentDictionary <int, ItemEntity> LoadItemsLocatedAt (ItemEntity location, Flags ignoreFlag = Flags.None)
    {
        ConcurrentDictionary <int, ItemEntity> result = new ConcurrentDictionary <int, ItemEntity> ();

        foreach (int itemID in ItemDB.LoadItemsLocatedAt (location.ID, ignoreFlag))
            result [itemID] = this.LoadItem (itemID);

        return result;
    }

    public ConcurrentDictionary <int, ItemEntity> LoadItemsLocatedAtByOwner (ItemEntity location, int ownerID, Flags itemFlag)
    {
        ConcurrentDictionary <int, ItemEntity> result = new ConcurrentDictionary <int, ItemEntity> ();

        foreach (int itemID in ItemDB.LoadItemsLocatedAtByOwner (location.ID, ownerID, itemFlag))
            result [itemID] = this.LoadItem (itemID);

        return result;
    }

    public bool IsItemLoaded (int itemID)
    {
        lock (this)
        {
            return this.mItemList.ContainsKey (itemID);
        }
    }

    private ItemEntity LoadSystem (Item item)
    {
        return new EVESystem (item);
    }

    private ItemEntity LoadCelestial (Item item)
    {
        switch (item.Type.Group.ID)
        {
            case (int) EVE.StaticData.Inventory.Groups.SolarSystem:
                return this.LoadSolarSystem (item);
            case (int) EVE.StaticData.Inventory.Groups.Station:
                return this.LoadStation (item);
            case (int) EVE.StaticData.Inventory.Groups.Constellation:
                return this.LoadConstellation (item);
            case (int) EVE.StaticData.Inventory.Groups.Region:
                return this.LoadRegion (item);
            case (int) EVE.StaticData.Inventory.Groups.CargoContainer:
            case (int) EVE.StaticData.Inventory.Groups.SecureCargoContainer:
            case (int) EVE.StaticData.Inventory.Groups.AuditLogSecureContainer:
            case (int) EVE.StaticData.Inventory.Groups.FreightContainer:
            case (int) EVE.StaticData.Inventory.Groups.Tool:
                return this.LoadContainer (item);
            default:
                Log.Warning ($"Loading celestial {item.ID} from item group {item.Type.Group.ID} as normal item");

                return new Items.Types.Item (item);
        }
    }

    private ItemEntity LoadBlueprint (Item item)
    {
        return new Blueprint (ItemDB.LoadBlueprint (item));
    }

    private ItemEntity LoadOwner (Item item)
    {
        switch (item.Type.Group.ID)
        {
            case (int) EVE.StaticData.Inventory.Groups.Character:
                return new Character (ItemDB.LoadCharacter (item));
            case (int) EVE.StaticData.Inventory.Groups.Corporation:
                return new Corporation (ItemDB.LoadCorporation (item));
            case (int) EVE.StaticData.Inventory.Groups.Faction:
                return this.LoadFaction (item);
            case (int) EVE.StaticData.Inventory.Groups.Alliance:
                return new Alliance (ItemDB.LoadAlliance (item));
            default:
                Log.Warning ($"Loading owner {item.ID} from item group {item.Type.Group.ID} as normal item");

                return new Items.Types.Item (item);
        }
    }

    private ItemEntity LoadFaction (Item item)
    {
        return Factions [item.ID] = new Faction (ItemDB.LoadFaction (item));
    }

    private ItemEntity LoadAccessories (Item item)
    {
        switch (item.Type.Group.ID)
        {
            case (int) EVE.StaticData.Inventory.Groups.Clone:
                return new Clone (item);
            default:
                Log.Warning ($"Loading accessory {item.ID} from item group {item.Type.Group.ID} as normal item");

                return new Items.Types.Item (item);
        }
    }

    private ItemEntity LoadSkill (Item item)
    {
        return new Skill (item, Constants.SkillPointMultiplier);
    }

    private ItemEntity LoadShip (Item item)
    {
        return new Ship (item);
    }

    private Implant LoadImplant (Item item)
    {
        return new Implant (item);
    }

    private ItemEntity LoadSolarSystem (Item item)
    {
        return SolarSystems [item.ID] = new SolarSystem (ItemDB.LoadSolarSystem (item));
    }

    private ItemEntity LoadStation (Item item)
    {
        switch (item.Type.Group.ID)
        {
            case (int) EVE.StaticData.Inventory.Groups.StationServices:
                return this.LoadStationServices (item);
            case (int) EVE.StaticData.Inventory.Groups.Station:
                return Stations [item.ID] = new Station (ItemDB.LoadStation (item));
            default:
                Log.Warning ($"Loading station item {item.ID} from item group {item.Type.Group.ID} as normal item");

                return new Items.Types.Item (item);
        }
    }

    private ItemEntity LoadStationServices (Item item)
    {
        switch (item.Type.ID)
        {
            case (int) Types.OfficeFolder:
                return new OfficeFolder (item);
            default:
                Log.Warning ($"Loading station service item {item.ID} as normal item");

                return null;
        }
    }

    private ItemEntity LoadConstellation (Item item)
    {
        return new Constellation (ItemDB.LoadConstellation (item));
    }

    private ItemEntity LoadRegion (Item item)
    {
        return new Region (ItemDB.LoadRegion (item));
    }

    private ItemEntity LoadContainer (Item item)
    {
        return new Items.Types.Container (item);
    }

    private ShipModule LoadModule (Item item)
    {
        return new ShipModule (item);
    }

    public ItemEntity CreateSimpleItem (
        Type type,               int  owner, int location, Flags flag, int quantity = 1,
        bool contraband = false, bool singleton = false
    )
    {
        return this.CreateSimpleItem (null, type.ID, owner, location, flag, quantity, contraband, singleton);
    }

    public ItemEntity CreateSimpleItem (
        string itemName,       int  typeID,             int  ownerID,           int    locationID, Flags  flag,
        int    quantity   = 1, bool contraband = false, bool singleton = false, double x = 0.0,    double y = 0.0, double z = 0.0,
        string customInfo = null
    )
    {
        int itemID = (int) ItemDB.CreateItem (
            itemName, typeID, ownerID, locationID, flag, contraband, singleton,
            quantity, x, y, z, customInfo
        );

        return this.LoadItem (itemID);
    }

    public ItemEntity CreateSimpleItem (
        string itemName,           Type type,              ItemEntity owner,        ItemEntity location, Flags flag,
        bool   contraband = false, bool singleton = false, int        quantity = 1, double     x = 0.0, double y = 0.0, double z = 0.0, string customInfo = null
    )
    {
        return this.CreateSimpleItem (
            itemName, type.ID, owner.ID, location.ID, flag, quantity, contraband, singleton,
            x, y, z, customInfo
        );
    }

    public ItemEntity CreateSimpleItem (
        Type type,         ItemEntity owner,              ItemEntity location, Flags flags,
        int  quantity = 1, bool       contraband = false, bool       singleton = false
    )
    {
        return this.CreateSimpleItem (null, type, owner, location, flags, contraband, singleton, quantity);
    }

    public Skill CreateSkill (Type skillType, Character character, int level = 0, SkillHistoryReason reason = SkillHistoryReason.SkillTrainingComplete)
    {
        int skillID = SkillDB.CreateSkill (skillType, character);

        Skill skill = this.LoadItem <Skill> (skillID);

        // update skill level
        skill.Level = level;

        // add skill to the character's inventory
        character.AddItem (skill);

        // create a history entry if needed
        if (reason != SkillHistoryReason.None)
            SkillDB.CreateSkillHistoryRecord (skillType, character, reason, skill.GetSkillPointsForLevel (level));

        // persist the skill to the database
        skill.Persist ();

        return skill;
    }

    public Ship CreateShip (Type shipType, ItemEntity location, Character owner)
    {
        int shipID = (int) ItemDB.CreateShip (shipType, location, owner);

        Ship ship = this.LoadItem <Ship> (shipID);

        return ship;
    }

    public Clone CreateClone (Type cloneType, ItemEntity location, Character owner)
    {
        return this.CreateSimpleItem (cloneType, owner, location, Flags.Clone, 1, false, true) as Clone;
    }

    public void UnloadItem (ItemEntity item)
    {
        lock (this)
        {
            // first ensure there's no meta inventory holding this item hostage
            if (MetaInventoryManager.GetOwnerInventoryAtLocation (item.ID, item.OwnerID, item.Flag, out ItemInventoryByOwnerID _))
                return;

            if (this.mItemList.Remove (item.ID) == false)
                return;

            // dispose of it
            item.Dispose ();
        }
    }

    public void UnloadItem (int itemID)
    {
        if (this.TryGetItem (itemID, out ItemEntity item) == false)
            return;

        this.UnloadItem (item);
    }

    public void DestroyItem (ItemEntity item)
    {
        lock (this)
        {
            // remove the item off the list and throw an exception if the item wasn't loaded in this item manager
            if (this.mItemList.Remove (item.ID) == false)
                throw new ArgumentException ("Cannot destroy an item that was not loaded by this item manager");

            // ensure the meta inventories know this item is not there anymore
            MetaInventoryManager.OnItemDestroyed (item);

            // make sure the location it's at knows the item is no more
            if (this.TryGetItem (item.LocationID, out ItemEntity location) && location is ItemInventory inventory)
                inventory.RemoveItem (item);

            // set the item to the recycler location just in case something has a reference to it somewhere
            item.LocationID = LocationRecycler.ID;
            // item.Flag = ItemFlags.None;

            // finally remove the item off the database
            item.Destroy ();
        }
    }

    private ItemEntity SubscribeToEvents (ItemEntity origin)
    {
        origin.OnItemDestroyed += this.OnItemDestroyed;
        origin.OnItemDisposed  += this.OnItemDisposed;
        origin.OnItemPersisted += this.OnItemPersisted;

        if (origin is ItemInventory inventory)
            this.SubscribeToInventoryEvents (inventory);

        if (origin is Character character)
            character.OnSkillQueueLoad += this.OnSkillQueueLoad;

        return origin;
    }

    private void SubscribeToInventoryEvents (ItemInventory inventory)
    {
        // extra events for inventories
        inventory.OnInventoryLoad   += this.OnInventoryLoad;
        inventory.OnInventoryUnload += this.OnInventoryUnload;
    }

    private void OnMetaInventoryCreated (ItemInventoryByOwnerID metaInventory)
    {
        // subscribe to the inventory events
        this.SubscribeToInventoryEvents (metaInventory);
    }

    private List <Character.SkillQueueEntry> OnSkillQueueLoad (Character character, Dictionary <int, Skill> skillQueue)
    {
        return CharacterDB.LoadSkillQueue (character, skillQueue);
    }

    private ConcurrentDictionary <int, ItemEntity> OnInventoryLoad (ItemInventory inventory, Flags ignoreFlags)
    {
        if (inventory is ItemInventoryByOwnerID byOwner)
            return this.LoadItemsLocatedAtByOwner (byOwner, byOwner.OwnerID, byOwner.InventoryFlag);

        return this.LoadItemsLocatedAt (inventory, ignoreFlags);
    }

    private void OnInventoryUnload (ItemInventory inventory)
    {
        foreach ((int _, ItemEntity item) in inventory.Items)
            this.UnloadItem (item);
    }

    private void OnItemDestroyed (ItemEntity item)
    {
        // delete items of an inventory too
        // this will trigger the load of the inventory
        // but it's a necessary evil for now
        // TODO: DO NOT LOAD THE ITEMS JUST FOR REMOVING THINGS?
        if (item is ItemInventory inventory)
            this.DestroyItems (inventory.Items);

        // TODO: ADD SPECIAL HANDLING FOR ITEMS THAT NEED EXTRA CLEANUP
        ItemDB.DestroyItem (item);
    }

    private void OnItemPersisted (ItemEntity item)
    {
        // TODO: ADD SPECIAL HANDLING FOR ITEMS THAT HAVE EXTRA DATA
        if (item.Information.New || item.Information.Dirty)
        {
            ItemDB.PersistEntity (item);

            switch (item)
            {
                case Alliance alliance:
                    this.PersistAlliance (alliance);
                    break;

                case Blueprint blueprint:
                    ItemDB.PersistBlueprint (blueprint.BlueprintInformation);
                    break;

                case Ship ship:
                    InsuranceDB.UnInsureShip (ship.ID);
                    break;

                case Corporation corporation:
                    CorporationDB.UpdateCorporationInformation (corporation);
                    break;

                case Character character:
                    CharacterDB.UpdateCharacterInformation (character);
                    break;

            }
        }

        // for inventories save every item too
        if (item is ItemInventory inventory && inventory.ContentsLoaded)
            foreach ((int _, ItemEntity inventoryItem) in inventory.Items)
                inventoryItem.Persist ();

        // persist the attributes too
        ItemDB.PersistAttributeList (item, item.Attributes);
    }

    private void OnItemDisposed (ItemEntity item)
    {
        // update the ownership information
        ItemDB.UnloadItem (item.ID);
    }

    public void DestroyItems (ConcurrentDictionary <int, ItemEntity> items)
    {
        foreach (KeyValuePair <int, ItemEntity> pair in items)
            this.DestroyItem (pair.Value);
    }

    public void DestroyItems (Dictionary <int, ItemEntity> items)
    {
        foreach (KeyValuePair <int, ItemEntity> pair in items)
            this.DestroyItem (pair.Value);
    }


    public void DestroyItems (List <ItemEntity> items)
    {
        foreach (ItemEntity item in items)
            this.DestroyItem (item);
    }

    private void PersistAlliance (Alliance alliance)
    {
        // update the alliance information
        Database.CrpAlliancesUpdate (alliance.Description, alliance.Url, alliance.ID, alliance.ExecutorCorpID);
    }
}