using System;
using System.Threading;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Notifications.Inventory;
using EVESharp.Node.Notifications;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using Attribute = EVESharp.EVE.Data.Inventory.Attributes.Attribute;

namespace EVESharp.Node.Dogma;

/// <summary>
/// Utility class for Dogma-related stuff
/// </summary>
public class DogmaNotifications : IDogmaNotifications
{
    /// <summary>
    /// List of notifications pending by session
    /// </summary>
    private readonly PyDictionary <PyTuple, PyList <PyTuple>> mPendingEvents = new PyDictionary <PyTuple, PyList <PyTuple>> ();

    /// <summary>
    /// The thread that will handle Dogma notification sending
    /// </summary>
    private readonly Thread mThread;

    /// <summary>
    /// Notification manager used to send notifications
    /// </summary>
    private INotificationSender Notifications { get; }

    public DogmaNotifications (INotificationSender notificationSender)
    {
        Notifications = notificationSender;
        this.mThread  = new Thread (this.Tick);
        this.mThread.Start ();
    }

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange (int ownerID, Attribute attribute, ItemEntity item)
    {
        this.QueueMultiEvent (
            NotificationIdType.Owner,
            ownerID,
            this.PrepareNotifyAttributeChange (attribute, item)
        );
    }

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange (int ownerID, int locationID, Attribute attribute, ItemEntity item)
    {
        this.QueueMultiEvent (
            NotificationIdType.OwnerAndLocation,
            new PyTuple (2)
            {
                [0] = ownerID,
                [1] = locationID
            },
            this.PrepareNotifyAttributeChange (attribute, item)
        );
    }

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange (NotificationIdType idType, int value, Attribute attribute, ItemEntity item)
    {
        this.QueueMultiEvent (
            idType,
            value,
            this.PrepareNotifyAttributeChange (attribute, item)
        );
    }

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange (NotificationIdType idType, PyTuple value, Attribute attribute, ItemEntity item)
    {
        this.QueueMultiEvent (
            idType,
            value,
            this.PrepareNotifyAttributeChange (attribute, item)
        );
    }

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange (NotificationIdType idType, PyList <PyInteger> value, Attribute attribute, ItemEntity item)
    {
        this.QueueMultiEvent (
            idType,
            value,
            this.PrepareNotifyAttributeChange (attribute, item)
        );
    }

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange (NotificationIdType idType, PyList <PyTuple> value, Attribute attribute, ItemEntity item)
    {
        this.QueueMultiEvent (
            idType,
            value,
            this.PrepareNotifyAttributeChange (attribute, item)
        );
    }

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange (int ownerID, AttributeTypes attribute, ItemEntity item)
    {
        this.QueueMultiEvent (
            NotificationIdType.Owner,
            ownerID,
            this.PrepareNotifyAttributeChange (attribute, item)
        );
    }

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange (int ownerID, int locationID, AttributeTypes attribute, ItemEntity item)
    {
        this.QueueMultiEvent (
            NotificationIdType.OwnerAndLocation,
            new PyTuple (2)
            {
                [0] = ownerID,
                [1] = locationID
            },
            this.PrepareNotifyAttributeChange (attribute, item)
        );
    }

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange (NotificationIdType idType, int value, AttributeTypes attribute, ItemEntity item)
    {
        this.QueueMultiEvent (
            idType,
            value,
            this.PrepareNotifyAttributeChange (attribute, item)
        );
    }

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange (NotificationIdType idType, PyTuple value, AttributeTypes attribute, ItemEntity item)
    {
        this.QueueMultiEvent (
            idType,
            value,
            this.PrepareNotifyAttributeChange (attribute, item)
        );
    }

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange (NotificationIdType idType, PyList <PyInteger> value, AttributeTypes attribute, ItemEntity item)
    {
        this.QueueMultiEvent (
            idType,
            value,
            this.PrepareNotifyAttributeChange (attribute, item)
        );
    }

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange (NotificationIdType idType, PyList <PyTuple> value, AttributeTypes attribute, ItemEntity item)
    {
        this.QueueMultiEvent (
            idType,
            value,
            this.PrepareNotifyAttributeChange (attribute, item)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on a specific item
    /// </summary>
    /// <param name="attributes">The attributes to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange (int ownerID, AttributeTypes [] attributes, ItemEntity item)
    {
        this.QueueMultiEvent (
            NotificationIdType.Owner,
            ownerID,
            this.PrepareNotifyAttributeChange (attributes, item)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on a specific item
    /// </summary>
    /// <param name="attributes">The attributes to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange (int ownerID, int locationID, AttributeTypes [] attributes, ItemEntity item)
    {
        this.QueueMultiEvent (
            NotificationIdType.OwnerAndLocation,
            new PyTuple (2)
            {
                [0] = ownerID,
                [1] = locationID
            },
            this.PrepareNotifyAttributeChange (attributes, item)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on a specific item
    /// </summary>
    /// <param name="attributes">The attributes to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange (NotificationIdType idType, int value, AttributeTypes [] attributes, ItemEntity item)
    {
        this.QueueMultiEvent (
            idType,
            value,
            this.PrepareNotifyAttributeChange (attributes, item)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on a specific item
    /// </summary>
    /// <param name="attributes">The attributes to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange (NotificationIdType idType, PyTuple value, AttributeTypes [] attributes, ItemEntity item)
    {
        this.QueueMultiEvent (
            idType,
            value,
            this.PrepareNotifyAttributeChange (attributes, item)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on a specific item
    /// </summary>
    /// <param name="attributes">The attributes to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange (NotificationIdType idType, PyList <PyInteger> value, AttributeTypes [] attributes, ItemEntity item)
    {
        this.QueueMultiEvent (
            idType,
            value,
            this.PrepareNotifyAttributeChange (attributes, item)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on a specific item
    /// </summary>
    /// <param name="attributes">The attributes to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange (NotificationIdType idType, PyList <PyTuple> value, AttributeTypes [] attributes, ItemEntity item)
    {
        this.QueueMultiEvent (
            idType,
            value,
            this.PrepareNotifyAttributeChange (attributes, item)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on different items
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="items">The list of items those attributes belong to</param>
    /// <exception cref="System.ArgumentOutOfRangeException">If the list of attributes and items is not the same size</exception>
    public void NotifyAttributeChange (int ownerID, Attribute [] attributes, ItemEntity [] items)
    {
        this.QueueMultiEvent (
            NotificationIdType.Owner,
            ownerID,
            this.PrepareNotifyAttributeChange (attributes, items)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on different items
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="items">The list of items those attributes belong to</param>
    /// <exception cref="System.ArgumentOutOfRangeException">If the list of attributes and items is not the same size</exception>
    public void NotifyAttributeChange (int ownerID, int locationID, Attribute [] attributes, ItemEntity [] items)
    {
        this.QueueMultiEvent (
            NotificationIdType.OwnerAndLocation,
            new PyTuple (2)
            {
                [0] = ownerID,
                [1] = locationID
            },
            this.PrepareNotifyAttributeChange (attributes, items)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on different items
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="items">The list of items those attributes belong to</param>
    /// <exception cref="System.ArgumentOutOfRangeException">If the list of attributes and items is not the same size</exception>
    public void NotifyAttributeChange (NotificationIdType idType, int value, Attribute [] attributes, ItemEntity [] items)
    {
        this.QueueMultiEvent (
            idType,
            value,
            this.PrepareNotifyAttributeChange (attributes, items)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on different items
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="items">The list of items those attributes belong to</param>
    /// <exception cref="System.ArgumentOutOfRangeException">If the list of attributes and items is not the same size</exception>
    public void NotifyAttributeChange (NotificationIdType idType, PyTuple value, Attribute [] attributes, ItemEntity [] items)
    {
        this.QueueMultiEvent (
            idType,
            value,
            this.PrepareNotifyAttributeChange (attributes, items)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on different items
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="items">The list of items those attributes belong to</param>
    /// <exception cref="System.ArgumentOutOfRangeException">If the list of attributes and items is not the same size</exception>
    public void NotifyAttributeChange (NotificationIdType idType, PyList <PyInteger> value, Attribute [] attributes, ItemEntity [] items)
    {
        this.QueueMultiEvent (
            idType,
            value,
            this.PrepareNotifyAttributeChange (attributes, items)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on different items
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="items">The list of items those attributes belong to</param>
    /// <exception cref="System.ArgumentOutOfRangeException">If the list of attributes and items is not the same size</exception>
    public void NotifyAttributeChange (NotificationIdType idType, PyList <PyTuple> value, Attribute [] attributes, ItemEntity [] items)
    {
        this.QueueMultiEvent (
            idType,
            value,
            this.PrepareNotifyAttributeChange (attributes, items)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on an item
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="item">The item these attributes belong to</param>
    public void NotifyAttributeChange (int ownerID, Attribute [] attributes, ItemEntity item)
    {
        this.QueueMultiEvent (
            ownerID,
            this.PrepareNotifyAttributeChange (attributes, item)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on an item
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="item">The item these attributes belong to</param>
    public void NotifyAttributeChange (int ownerID, int locationID, Attribute [] attributes, ItemEntity item)
    {
        this.QueueMultiEvent (
            NotificationIdType.OwnerAndLocation,
            new PyTuple (2)
            {
                [0] = ownerID,
                [1] = locationID
            },
            this.PrepareNotifyAttributeChange (attributes, item)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on an item
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="item">The item these attributes belong to</param>
    public void NotifyAttributeChange (NotificationIdType idType, int value, Attribute [] attributes, ItemEntity item)
    {
        this.QueueMultiEvent (
            idType,
            value,
            this.PrepareNotifyAttributeChange (attributes, item)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on an item
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="item">The item these attributes belong to</param>
    public void NotifyAttributeChange (NotificationIdType idType, PyTuple value, Attribute [] attributes, ItemEntity item)
    {
        this.QueueMultiEvent (
            idType,
            value,
            this.PrepareNotifyAttributeChange (attributes, item)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on an item
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="item">The item these attributes belong to</param>
    public void NotifyAttributeChange (NotificationIdType idType, PyList <PyInteger> value, Attribute [] attributes, ItemEntity item)
    {
        this.QueueMultiEvent (
            idType,
            value,
            this.PrepareNotifyAttributeChange (attributes, item)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on an item
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="item">The item these attributes belong to</param>
    public void NotifyAttributeChange (NotificationIdType idType, PyList <PyTuple> value, Attribute [] attributes, ItemEntity item)
    {
        this.QueueMultiEvent (
            idType,
            value,
            this.PrepareNotifyAttributeChange (attributes, item)
        );
    }

    public void NotifyAttributeChange (int ownerID, AttributeTypes attribute, ItemEntity [] items)
    {
        this.QueueMultiEvent (
            NotificationIdType.Owner,
            ownerID,
            this.PrepareNotifyAttributeChange (attribute, items)
        );
    }

    public void NotifyAttributeChange (int ownerID, int locationID, AttributeTypes attribute, ItemEntity [] items)
    {
        this.QueueMultiEvent (
            NotificationIdType.OwnerAndLocation,
            new PyTuple (2)
            {
                [0] = ownerID,
                [1] = locationID
            },
            this.PrepareNotifyAttributeChange (attribute, items)
        );
    }

    public void NotifyAttributeChange (NotificationIdType idType, int value, AttributeTypes attribute, ItemEntity [] items)
    {
        this.QueueMultiEvent (
            idType,
            value,
            this.PrepareNotifyAttributeChange (attribute, items)
        );
    }

    public void NotifyAttributeChange (NotificationIdType idType, PyTuple value, AttributeTypes attribute, ItemEntity [] items)
    {
        this.QueueMultiEvent (
            idType,
            value,
            this.PrepareNotifyAttributeChange (attribute, items)
        );
    }

    public void NotifyAttributeChange (NotificationIdType idType, PyList <PyInteger> value, AttributeTypes attribute, ItemEntity [] items)
    {
        this.QueueMultiEvent (
            idType,
            value,
            this.PrepareNotifyAttributeChange (attribute, items)
        );
    }

    public void NotifyAttributeChange (NotificationIdType idType, PyList <PyTuple> value, AttributeTypes attribute, ItemEntity [] items)
    {
        this.QueueMultiEvent (
            idType,
            value,
            this.PrepareNotifyAttributeChange (attribute, items)
        );
    }

    private OnModuleAttributeChange PrepareNotifyAttributeChange (AttributeTypes attribute, ItemEntity item)
    {
        return new OnModuleAttributeChange (item, item.Attributes [attribute]);
    }

    private OnModuleAttributeChanges PrepareNotifyAttributeChange (AttributeTypes [] attributes, ItemEntity item)
    {
        OnModuleAttributeChanges changes = new OnModuleAttributeChanges ();

        foreach (AttributeTypes attribute in attributes)
            changes.AddChange (this.PrepareNotifyAttributeChange (attribute, item));

        return changes;
    }

    private OnModuleAttributeChange PrepareNotifyAttributeChange (Attribute attribute, ItemEntity item)
    {
        return new OnModuleAttributeChange (item, attribute);
    }

    private OnModuleAttributeChanges PrepareNotifyAttributeChange (AttributeTypes attribute, ItemEntity [] items)
    {
        OnModuleAttributeChanges changes = new OnModuleAttributeChanges ();

        foreach (ItemEntity item in items)
            changes.AddChange (this.PrepareNotifyAttributeChange (attribute, item));

        return changes;
    }

    private OnModuleAttributeChanges PrepareNotifyAttributeChange (Attribute [] attributes, ItemEntity item)
    {
        OnModuleAttributeChanges changes = new OnModuleAttributeChanges ();

        foreach (Attribute attribute in attributes)
            changes.AddChange (new OnModuleAttributeChange (item, attribute));

        return changes;
    }

    private OnModuleAttributeChanges PrepareNotifyAttributeChange (Attribute [] attributes, ItemEntity [] items)
    {
        if (attributes.Length != items.Length)
            throw new ArgumentOutOfRangeException (
                nameof (attributes),
                "attributes list and items list must have the same amount of elements"
            );

        OnModuleAttributeChanges changes = new OnModuleAttributeChanges ();

        for (int i = 0; i < attributes.Length; i++)
            changes.AddChange (new OnModuleAttributeChange (items [i], attributes [i]));

        return changes;
    }

    /// <summary>
    /// Queues a multievent to be sent on the next dogma tick
    /// </summary>
    /// <param name="key">The character to notify</param>
    /// <param name="notif">The notification</param>
    private void QueueMultiEvent (PyTuple key, PyTuple notif)
    {
        lock (this.mPendingEvents)
        {
            // make sure there's an entry for this character
            if (this.mPendingEvents.TryGetValue (key, out PyList <PyTuple> events) == false)
            {
                events = new PyList <PyTuple> ();
                this.mPendingEvents.Add (key, events);
            }

            // add the notification to the queue
            events.Add (notif);
        }
    }

    /// <summary>
    /// Queues a multievent to be sent to the specified players on the next dogma tick
    /// </summary>
    /// <param name="idType">The type of id to use for notification</param>
    /// <param name="id">The value to notify</param>
    /// <param name="notif">The notification</param>
    public void QueueMultiEvent (NotificationIdType idType, int id, PyTuple notif)
    {
        PyList <PyInteger> ids = new PyList <PyInteger> {id};

        this.QueueMultiEvent (
            new PyTuple (2)
            {
                [0] = INotificationSender.NotificationTypeTranslation [idType],
                [1] = ids
            }, notif
        );
    }

    /// <summary>
    /// Queues a multievent to be sent to the specified players on the next dogma tick
    /// </summary>
    /// <param name="idType">The type of id to use for notification</param>
    /// <param name="id">The value to notify</param>
    /// <param name="notif">The notification</param>
    public void QueueMultiEvent (NotificationIdType idType, PyTuple id, PyTuple notif)
    {
        PyList <PyTuple> ids = new PyList <PyTuple> {id};

        this.QueueMultiEvent (
            new PyTuple (2)
            {
                [0] = INotificationSender.NotificationTypeTranslation [idType],
                [1] = ids
            }, notif
        );
    }

    /// <summary>
    /// Queues a multievent to be sent to the specified players on the next dogma tick
    /// </summary>
    /// <param name="idType"></param>
    /// <param name="ids"></param>
    /// <param name="notif"></param>
    public void QueueMultiEvent (NotificationIdType idType, PyList <PyInteger> ids, PyTuple notif)
    {
        this.QueueMultiEvent (
            new PyTuple (2)
            {
                [0] = INotificationSender.NotificationTypeTranslation [idType],
                [1] = ids
            }, notif
        );
    }

    /// <summary>
    /// Queues a multievent to be sent to the specified players on the next dogma tick
    /// </summary>
    /// <param name="idType"></param>
    /// <param name="ids"></param>
    /// <param name="notif"></param>
    public void QueueMultiEvent (NotificationIdType idType, PyList <PyTuple> ids, PyTuple notif)
    {
        this.QueueMultiEvent (
            new PyTuple (2)
            {
                [0] = INotificationSender.NotificationTypeTranslation [idType],
                [1] = ids
            }, notif
        );
    }

    /// <summary>
    /// Queues a multievent to be sent to the given ownerID on the next dogma tick
    /// </summary>
    /// <param name="ownerID">The character to notify</param>
    /// <param name="notif">The notification's body</param>
    public void QueueMultiEvent (int ownerID, PyTuple notif)
    {
        this.QueueMultiEvent (NotificationIdType.Owner, ownerID, notif);
    }

    /// <summary>
    /// Sends all the queued multievents
    /// </summary>
    private void SendMultiEvents ()
    {
        lock (this.mPendingEvents)
        {
            foreach ((PyTuple type, PyList <PyTuple> notifs) in this.mPendingEvents)
                Notifications.SendNotification (
                    "OnMultiEvent",
                    type [0] as PyString,
                    type [1] as PyList,
                    new PyTuple (1) {[0] = notifs}
                );

            this.mPendingEvents.Clear ();
        }
    }

    private void Tick ()
    {
        while (true)
        {
            // for now just do some wait and flush the multi-event queue
            Thread.Sleep (100);

            this.SendMultiEvents ();
        }
    }
}