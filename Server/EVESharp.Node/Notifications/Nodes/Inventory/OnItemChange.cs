using System;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Notifications.Nodes.Inventory;

public class OnItemChange : InterNodeNotification
{
    public const string NOTIFICATION_NAME = "OnItemChange";

    public PyDictionary <PyInteger, PyDictionary> Updates { get; init; }

    public OnItemChange () : base (NOTIFICATION_NAME)
    {
        Updates = new PyDictionary <PyInteger, PyDictionary> ();
    }

    protected OnItemChange (PyDictionary <PyInteger, PyDictionary> updates) : base (NOTIFICATION_NAME)
    {
        Updates = updates;
    }

    public OnItemChange AddChange (int itemID, string updatedValue, PyDataType oldValue, PyDataType newValue)
    {
        if (Updates.TryGetValue (itemID, out PyDictionary changes) == false)
            changes = Updates [itemID] = new PyDictionary <PyString, PyTuple> ();

        changes [updatedValue] = new PyTuple (2)
        {
            [0] = oldValue,
            [1] = newValue
        };

        return this;
    }

    public static OnItemChange BuildQuantityChange (int itemID, int oldQuantity, int newQuantity)
    {
        return new OnItemChange ().AddChange (itemID, "quantity", oldQuantity, newQuantity);
    }

    public static OnItemChange BuildLocationChange (int itemID, Flags oldFlag, Flags newFlag)
    {
        return new OnItemChange ().AddChange (itemID, "flag", (int) oldFlag, (int) newFlag);
    }

    public static OnItemChange BuildLocationChange (int itemID, int oldLocation, int newLocation)
    {
        return new OnItemChange ().AddChange (itemID, "locationID", oldLocation, newLocation);
    }

    public static OnItemChange BuildLocationChange (int itemID, Flags oldFlag, Flags newFlag, int oldLocation, int newLocation)
    {
        OnItemChange change = new OnItemChange ();

        if (oldFlag != newFlag)
            change.AddChange (itemID, "flag", (int) oldFlag, (int) newFlag);

        if (oldLocation != newLocation)
            change.AddChange (itemID, "locationID", oldLocation, newLocation);

        return change;
    }

    public static OnItemChange BuildNewItemChange (int itemID, int locationID)
    {
        // new items are notified as being moved from location 0 to the actual location
        return BuildLocationChange (itemID, 0, locationID);
    }

    public static OnItemChange BuildSingletonChange (int itemID, bool oldSingleton, bool newSingleton)
    {
        return new OnItemChange ().AddChange (itemID, "singleton", oldSingleton, newSingleton);
    }

    protected override PyDataType GetNotification ()
    {
        return Updates;
    }

    public static implicit operator OnItemChange (PyTuple notification)
    {
        if (notification.Count != 2)
            throw new InvalidCastException ("Expected a tuple with one item");

        if (notification [0] is not PyString name || name != NOTIFICATION_NAME)
            throw new InvalidCastException ($"Expected a {NOTIFICATION_NAME}");

        if (notification [1] is not PyDictionary list)
            throw new InvalidCastException ("Expected a list as the first element");

        return new OnItemChange (list.GetEnumerable <PyInteger, PyDictionary> ());
    }
}