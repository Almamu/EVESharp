﻿using System.Collections.Generic;
using EVESharp.Database.Inventory;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Packets.Complex;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Notifications.Inventory;

public class OnItemChange : ClientNotification
{
    /// <summary>
    /// The notification name
    /// </summary>
    private const string NOTIFICATION_NAME = "OnItemChange";
    public ItemEntity                          Item    { get; }
    public Dictionary <ItemChange, PyDataType> Changes { get; } = new Dictionary <ItemChange, PyDataType> ();

    public OnItemChange (ItemEntity item) : base (NOTIFICATION_NAME)
    {
        this.Item = item;
    }

    /// <summary>
    /// Adds a change to the list of changes this notification will tell the client
    /// </summary>
    /// <param name="change">The type of change that happened</param>
    /// <param name="oldValue">The old value for this change</param>
    /// <returns>Itself so methods can be chained</returns>
    public OnItemChange AddChange (ItemChange change, PyDataType oldValue)
    {
        this.Changes [change] = oldValue;

        return this;
    }

    /// <summary>
    /// Builds the correct PyDictionary for the changes described by this notification
    /// </summary>
    /// <returns></returns>
    private PyDictionary <PyInteger, PyDataType> BuildChangeDictionary ()
    {
        PyDictionary <PyInteger, PyDataType> result = new PyDictionary <PyInteger, PyDataType> ();

        foreach ((ItemChange changeType, PyDataType oldValue) in this.Changes)
            result [(int) changeType] = oldValue;

        return result;
    }

    public override List <PyDataType> GetElements ()
    {
        return new List <PyDataType>
        {
            this.Item.GetEntityRow (),
            this.BuildChangeDictionary ()
        };
    }

    public static OnItemChange BuildQuantityChange (ItemEntity item, int oldQuantity)
    {
        return new OnItemChange (item).AddChange (ItemChange.Quantity, oldQuantity);
    }

    public static OnItemChange BuildLocationChange (ItemEntity item, Flags oldFlag)
    {
        return new OnItemChange (item).AddChange (ItemChange.Flag, (int) oldFlag);
    }

    public static OnItemChange BuildLocationChange (ItemEntity item, int? oldLocation)
    {
        return new OnItemChange (item).AddChange (ItemChange.LocationID, oldLocation);
    }

    public static OnItemChange BuildLocationChange (ItemEntity item, Flags oldFlag, int? oldLocation)
    {
        OnItemChange change = new OnItemChange (item);

        if (item.Flag != oldFlag)
            change.AddChange (ItemChange.Flag, (int) oldFlag);
        if (item.LocationID != oldLocation)
            change.AddChange (ItemChange.LocationID, oldLocation);

        return change;
    }

    public static OnItemChange BuildNewItemChange (ItemEntity item)
    {
        // new items are notified as being moved from location 0 to the actual location
        return BuildLocationChange (item, Flags.None, 0);
    }

    public static OnItemChange BuildSingletonChange (ItemEntity item, bool oldSingleton)
    {
        return new OnItemChange (item).AddChange (ItemChange.Singleton, oldSingleton);
    }
}