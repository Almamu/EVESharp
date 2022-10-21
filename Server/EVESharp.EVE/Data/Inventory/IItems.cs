using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using EVESharp.Database.Characters;
using EVESharp.Database.Dogma;
using EVESharp.Database.Inventory;
using EVESharp.Database.Inventory.Attributes;
using EVESharp.Database.Inventory.Categories;
using EVESharp.Database.Inventory.Characters;
using EVESharp.Database.Inventory.Groups;
using EVESharp.Database.Inventory.Types;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Data.Inventory.Items.Types;
using Type = EVESharp.Database.Inventory.Types.Type;

namespace EVESharp.EVE.Data.Inventory;

public interface IItems
{
    public IDefaultAttributes DefaultAttributes { get; }
    public IAttributes        Attributes        { get; }
    public ICategories        Categories        { get; }
    public IGroups            Groups            { get; }
    public ITypes             Types             { get; }
    public IStations          Stations          { get; }
    public ISolarSystems      SolarSystems      { get; }
    public IAncestries        Ancestries        { get; }
    public IBloodlines        Bloodlines        { get; }
    public IFactions          Factions          { get; }
    public IExpressions       Expressions       { get; }

    /// <summary>
    /// Event fired when a new item is loaded
    /// </summary>
    public event Action <ItemEntity> Loaded;

    /// <summary>
    /// Event fired when an item is unloaded
    /// </summary>
    public event Action <ItemEntity> Unloaded;

    /// <summary>
    /// Event fired when an item is destroyed
    /// </summary>
    public event Action <ItemEntity> Destroyed;

    public EVESystem                     OwnerBank         { get; }
    public EVESystem                     LocationSystem    { get; }
    public EVESystem                     LocationRecycler  { get; }
    public EVESystem                     LocationMarket    { get; }
    public EVESystem                     LocationUniverse  { get; }
    public EVESystem                     LocationTemp      { get; }
    public ItemEntity                    OwnerSCC          { get; }
    
    /// <summary>
    /// Initializes the item factory and loads the required items
    /// </summary>
    void Init ();

    /// <summary>
    /// Checks if an item exists and returns it
    /// </summary>
    /// <param name="itemID">The item to get</param>
    /// <param name="item">Where to put the item</param>
    /// <returns>Whether the item exists in the manager or not</returns>
    bool TryGetItem (int itemID, out ItemEntity item);

    /// <summary>
    /// Shorthand method to get an item of an specific type
    /// </summary>
    /// <param name="itemID"></param>
    /// <param name="item"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    bool TryGetItem <T> (int itemID, out T item) where T : ItemEntity;

    /// <summary>
    /// Loads an item if it's not already loaded and returns it'
    /// </summary>
    /// <param name="itemID">The item to load</param>
    /// <returns>The loaded item</returns>
    ItemEntity LoadItem (int itemID);

    /// <summary>
    /// Loads an item if it's not already loaded and returns it
    /// </summary>
    /// <param name="itemID">The item to load</param>
    /// <param name="loadRequired">Whether the item was loaded or already existed</param>
    /// <returns>The loaded item</returns>
    ItemEntity LoadItem (int itemID, out bool loadRequired);

    /// <summary>
    /// Loads an item if it's not already loaded and returns it'
    /// </summary>
    /// <param name="itemID">The item to load</param>
    /// <returns>The loaded item</returns>
    T LoadItem <T> (int itemID) where T : ItemEntity;

    T LoadItem <T> (int itemID, out bool loadRequired) where T : ItemEntity;

    /// <summary>
    /// Gets the given item
    /// </summary>
    /// <param name="itemID">The item to get</param>
    /// <returns>The item</returns>
    /// <exception cref="ItemNotLoadedException">If the item doesn't exist</exception>
    ItemEntity GetItem (int itemID);

    T GetItem <T> (int itemID) where T : ItemEntity;

    /// <summary>
    /// Gets the <see cref="ItemEntity"/> of the given faction
    /// </summary>
    /// <param name="factionID">The factionID to get</param>
    /// <returns>The item</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the id is not a faction</exception>
    Faction GetStaticFaction (int factionID);

    /// <summary>
    /// Gets the <see cref="ItemEntity"/> of the given station
    /// </summary>
    /// <param name="stationID">The stationID to get</param>
    /// <returns>The item</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the id is not a station</exception>
    Items.Types.Station GetStaticStation (int stationID);

    /// <summary>
    /// Gets the <see cref="ItemEntity"/> of the given solar system
    /// </summary>
    /// <param name="solarSystemID">The solarSystemID to get</param>
    /// <returns>The item</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the id is not a solar system</exception>
    SolarSystem GetStaticSolarSystem (int solarSystemID);

    /// <summary>
    /// Checks if the given item is already loaded
    /// </summary>
    /// <param name="itemID"></param>
    /// <returns></returns>
    bool IsItemLoaded (int itemID);

    ItemEntity CreateSimpleItem (
        Type type,               int  owner, int location, Flags flag, int quantity = 1,
        bool contraband = false, bool singleton = false
    );

    ItemEntity CreateSimpleItem (
        string itemName,       int  typeID,             int  ownerID,           int    locationID, Flags  flag,
        int    quantity   = 1, bool contraband = false, bool singleton = false, double x = 0.0,    double y = 0.0, double z = 0.0,
        string customInfo = null
    );

    ItemEntity CreateSimpleItem (
        string itemName,           Type type,              ItemEntity owner,        ItemEntity location, Flags flag,
        bool   contraband = false, bool singleton = false, int        quantity = 1, double     x = 0.0, double y = 0.0, double z = 0.0, string customInfo = null
    );

    ItemEntity CreateSimpleItem (
        Type type,         ItemEntity owner,              ItemEntity location, Flags flags,
        int  quantity = 1, bool       contraband = false, bool       singleton = false
    );

    Skill CreateSkill (Type                                    skillType, Character  character, int       level = 0, SkillHistoryReason reason = SkillHistoryReason.SkillTrainingComplete);
    Ship  CreateShip (Type                                     shipType,  ItemEntity location,  Character owner);
    Clone CreateClone (Type                                    cloneType, ItemEntity location,  Character owner);
    void  UnloadItem (ItemEntity                               item);
    void  UnloadItem (int                                      itemID);
    void  DestroyItem (ItemEntity                              item);
    void  DestroyItems (ConcurrentDictionary <int, ItemEntity> items);
}