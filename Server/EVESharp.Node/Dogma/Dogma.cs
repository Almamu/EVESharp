using System;
using System.Collections.Generic;
using System.Threading;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Network;
using EVESharp.Node.Notifications.Client.Inventory;
using EVESharp.Node.Sessions;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using Attribute = EVESharp.Node.Inventory.Items.Attributes.Attribute;

namespace EVESharp.Node.Dogma;

/// <summary>
/// Utility class for Dogma-related stuff
/// </summary>
public class Dogma
{
    /// <summary>
    /// List of notifications pending by session
    /// </summary>
    private readonly PyDictionary<PyTuple, PyList<PyTuple>> mPendingEvents = new PyDictionary<PyTuple, PyList<PyTuple>>();

    /// <summary>
    /// The thread that will handle Dogma notification sending
    /// </summary>
    private readonly Thread mThread;

    /// <summary>
    /// Notification manager used to send notifications
    /// </summary>
    private NotificationManager NotificationManager { get; }

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange(int ownerID, Attribute attribute, ItemEntity item)
    {
        this.QueueMultiEvent(
            NotificationManager.NOTIFICATION_TYPE_OWNER,
            ownerID,
            this.PrepareNotifyAttributeChange(attribute, item)
        );
    }

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange(int ownerID, int locationID, Attribute attribute, ItemEntity item)
    {
        this.QueueMultiEvent(
            NotificationManager.NOTIFICATION_TYPE_OWNER_LOCATIONID,
            new PyTuple(2) {[0] = ownerID, [1] = locationID},
            this.PrepareNotifyAttributeChange(attribute, item)
        );
    }

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange(string idType, int value, Attribute attribute, ItemEntity item)
    {
        this.QueueMultiEvent(
            idType,
            value,
            this.PrepareNotifyAttributeChange(attribute, item)
        );
    }

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange(string idType, PyTuple value, Attribute attribute, ItemEntity item)
    {
        this.QueueMultiEvent(
            idType,
            value,
            this.PrepareNotifyAttributeChange(attribute, item)
        );
    }

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange(string idType, PyList<PyInteger> value, Attribute attribute, ItemEntity item)
    {
        this.QueueMultiEvent(
            idType,
            value,
            this.PrepareNotifyAttributeChange(attribute, item)
        );
    }

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange(string idType, PyList<PyTuple> value, Attribute attribute, ItemEntity item)
    {
        this.QueueMultiEvent(
            idType,
            value,
            this.PrepareNotifyAttributeChange(attribute, item)
        );
    }

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange(int ownerID, Attributes attribute, ItemEntity item)
    {
        this.QueueMultiEvent(
            NotificationManager.NOTIFICATION_TYPE_OWNER,
            ownerID,
            this.PrepareNotifyAttributeChange(attribute, item)
        );
    }

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange(int ownerID, int locationID, Attributes attribute, ItemEntity item)
    {
        this.QueueMultiEvent(
            NotificationManager.NOTIFICATION_TYPE_OWNER_LOCATIONID,
            new PyTuple(2) {[0] = ownerID, [1] = locationID},
            this.PrepareNotifyAttributeChange(attribute, item)
        );
    }

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange(string idType, int value, Attributes attribute, ItemEntity item)
    {
        this.QueueMultiEvent(
            idType,
            value,
            this.PrepareNotifyAttributeChange(attribute, item)
        );
    }

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange(string idType, PyTuple value, Attributes attribute, ItemEntity item)
    {
        this.QueueMultiEvent(
            idType,
            value,
            this.PrepareNotifyAttributeChange(attribute, item)
        );
    }

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange(string idType, PyList<PyInteger> value, Attributes attribute, ItemEntity item)
    {
        this.QueueMultiEvent(
            idType,
            value,
            this.PrepareNotifyAttributeChange(attribute, item)
        );
    }

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange(string idType, PyList<PyTuple> value, Attributes attribute, ItemEntity item)
    {
        this.QueueMultiEvent(
            idType,
            value,
            this.PrepareNotifyAttributeChange(attribute, item)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on a specific item
    /// </summary>
    /// <param name="attributes">The attributes to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange(int ownerID, Attributes[] attributes, ItemEntity item)
    {
        this.QueueMultiEvent(
            NotificationManager.NOTIFICATION_TYPE_OWNER,
            ownerID,
            this.PrepareNotifyAttributeChange(attributes, item)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on a specific item
    /// </summary>
    /// <param name="attributes">The attributes to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange(int ownerID, int locationID, Attributes[] attributes, ItemEntity item)
    {
        this.QueueMultiEvent(
            NotificationManager.NOTIFICATION_TYPE_OWNER_LOCATIONID,
            new PyTuple(2) {[0] = ownerID, [1] = locationID},
            this.PrepareNotifyAttributeChange(attributes, item)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on a specific item
    /// </summary>
    /// <param name="attributes">The attributes to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange(string idType, int value, Attributes[] attributes, ItemEntity item)
    {
        this.QueueMultiEvent(
            idType,
            value,
            this.PrepareNotifyAttributeChange(attributes, item)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on a specific item
    /// </summary>
    /// <param name="attributes">The attributes to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange(string idType, PyTuple value, Attributes[] attributes, ItemEntity item)
    {
        this.QueueMultiEvent(
            idType,
            value,
            this.PrepareNotifyAttributeChange(attributes, item)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on a specific item
    /// </summary>
    /// <param name="attributes">The attributes to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange(string idType, PyList<PyInteger> value, Attributes[] attributes, ItemEntity item)
    {
        this.QueueMultiEvent(
            idType,
            value,
            this.PrepareNotifyAttributeChange(attributes, item)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on a specific item
    /// </summary>
    /// <param name="attributes">The attributes to notify about</param>
    /// <param name="item">The item to notify about</param>
    public void NotifyAttributeChange(string idType, PyList<PyTuple> value, Attributes[] attributes, ItemEntity item)
    {
        this.QueueMultiEvent(
            idType,
            value,
            this.PrepareNotifyAttributeChange(attributes, item)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on different items
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="items">The list of items those attributes belong to</param>
    /// <exception cref="ArgumentOutOfRangeException">If the list of attributes and items is not the same size</exception>
    public void NotifyAttributeChange(int ownerID, Attribute[] attributes, ItemEntity[] items)
    {
        this.QueueMultiEvent(
            NotificationManager.NOTIFICATION_TYPE_OWNER,
            ownerID,
            this.PrepareNotifyAttributeChange(attributes, items)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on different items
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="items">The list of items those attributes belong to</param>
    /// <exception cref="ArgumentOutOfRangeException">If the list of attributes and items is not the same size</exception>
    public void NotifyAttributeChange(int ownerID, int locationID, Attribute[] attributes, ItemEntity[] items)
    {
        this.QueueMultiEvent(
            NotificationManager.NOTIFICATION_TYPE_OWNER_LOCATIONID,
            new PyTuple(2){[0] = ownerID, [1] = locationID},
            this.PrepareNotifyAttributeChange(attributes, items)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on different items
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="items">The list of items those attributes belong to</param>
    /// <exception cref="ArgumentOutOfRangeException">If the list of attributes and items is not the same size</exception>
    public void NotifyAttributeChange(string idType, int value, Attribute[] attributes, ItemEntity[] items)
    {
        this.QueueMultiEvent(
            idType,
            value,
            this.PrepareNotifyAttributeChange(attributes, items)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on different items
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="items">The list of items those attributes belong to</param>
    /// <exception cref="ArgumentOutOfRangeException">If the list of attributes and items is not the same size</exception>
    public void NotifyAttributeChange(string idType, PyTuple value, Attribute[] attributes, ItemEntity[] items)
    {
        this.QueueMultiEvent(
            idType,
            value,
            this.PrepareNotifyAttributeChange(attributes, items)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on different items
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="items">The list of items those attributes belong to</param>
    /// <exception cref="ArgumentOutOfRangeException">If the list of attributes and items is not the same size</exception>
    public void NotifyAttributeChange(string idType, PyList<PyInteger> value, Attribute[] attributes, ItemEntity[] items)
    {
        this.QueueMultiEvent(
            idType,
            value,
            this.PrepareNotifyAttributeChange(attributes, items)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on different items
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="items">The list of items those attributes belong to</param>
    /// <exception cref="ArgumentOutOfRangeException">If the list of attributes and items is not the same size</exception>
    public void NotifyAttributeChange(string idType, PyList<PyTuple> value, Attribute[] attributes, ItemEntity[] items)
    {
        this.QueueMultiEvent(
            idType,
            value,
            this.PrepareNotifyAttributeChange(attributes, items)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on an item
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="item">The item these attributes belong to</param>
    public void NotifyAttributeChange(int ownerID, Attribute[] attributes, ItemEntity item)
    {
        this.QueueMultiEvent(
            ownerID,
            this.PrepareNotifyAttributeChange(attributes, item)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on an item
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="item">The item these attributes belong to</param>
    public void NotifyAttributeChange(int ownerID, int locationID, Attribute[] attributes, ItemEntity item)
    {
        this.QueueMultiEvent(
            NotificationManager.NOTIFICATION_TYPE_OWNER_LOCATIONID,
            new PyTuple(2){[0] = ownerID, [1] = locationID},
            this.PrepareNotifyAttributeChange(attributes, item)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on an item
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="item">The item these attributes belong to</param>
    public void NotifyAttributeChange(string idType, int value, Attribute[] attributes, ItemEntity item)
    {
        this.QueueMultiEvent(
            idType,
            value,
            this.PrepareNotifyAttributeChange(attributes, item)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on an item
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="item">The item these attributes belong to</param>
    public void NotifyAttributeChange(string idType, PyTuple value, Attribute[] attributes, ItemEntity item)
    {
        this.QueueMultiEvent(
            idType,
            value,
            this.PrepareNotifyAttributeChange(attributes, item)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on an item
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="item">The item these attributes belong to</param>
    public void NotifyAttributeChange(string idType, PyList<PyInteger> value, Attribute[] attributes, ItemEntity item)
    {
        this.QueueMultiEvent(
            idType,
            value,
            this.PrepareNotifyAttributeChange(attributes, item)
        );
    }

    /// <summary>
    /// Notifies the client of a multiple attribute change on an item
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="item">The item these attributes belong to</param>
    public void NotifyAttributeChange(string idType, PyList<PyTuple> value, Attribute[] attributes, ItemEntity item)
    {
        this.QueueMultiEvent(
            idType,
            value,
            this.PrepareNotifyAttributeChange(attributes, item)
        );
    }

    public void NotifyAttributeChange(int ownerID, Attributes attribute, ItemEntity[] items)
    {
        this.QueueMultiEvent(
            NotificationManager.NOTIFICATION_TYPE_OWNER,
            ownerID,
            this.PrepareNotifyAttributeChange(attribute, items)
        );
    }

    public void NotifyAttributeChange(int ownerID, int locationID, Attributes attribute, ItemEntity[] items)
    {
        this.QueueMultiEvent(
            NotificationManager.NOTIFICATION_TYPE_OWNER_LOCATIONID,
            new PyTuple(2) {[0] = ownerID, [1] = locationID},
            this.PrepareNotifyAttributeChange(attribute, items)
        );
    }

    public void NotifyAttributeChange(string idType, int value, Attributes attribute, ItemEntity[] items)
    {
        this.QueueMultiEvent(
            idType,
            value,
            this.PrepareNotifyAttributeChange(attribute, items)
        );
    }

    public void NotifyAttributeChange(string idType, PyTuple value, Attributes attribute, ItemEntity[] items)
    {
        this.QueueMultiEvent(
            idType,
            value,
            this.PrepareNotifyAttributeChange(attribute, items)
        );
    }

    public void NotifyAttributeChange(string idType, PyList<PyInteger> value, Attributes attribute, ItemEntity[] items)
    {
        this.QueueMultiEvent(
            idType,
            value,
            this.PrepareNotifyAttributeChange(attribute, items)
        );
    }

    public void NotifyAttributeChange(string idType, PyList<PyTuple> value, Attributes attribute, ItemEntity[] items)
    {
        this.QueueMultiEvent(
            idType,
            value,
            this.PrepareNotifyAttributeChange(attribute, items)
        );
    }

    private OnModuleAttributeChange PrepareNotifyAttributeChange(Attributes attribute, ItemEntity item)
    {
        return new OnModuleAttributeChange(item, item.Attributes[attribute]);
    }

    private OnModuleAttributeChanges PrepareNotifyAttributeChange(Attributes[] attributes, ItemEntity item)
    {
        OnModuleAttributeChanges changes = new OnModuleAttributeChanges();
        
        foreach (Attributes attribute in attributes)
            changes.AddChange(this.PrepareNotifyAttributeChange(attribute, item));

        return changes;
    }

    private OnModuleAttributeChange PrepareNotifyAttributeChange(Attribute attribute, ItemEntity item)
    {
        return new OnModuleAttributeChange(item, attribute);
    }
    
    private OnModuleAttributeChanges PrepareNotifyAttributeChange(Attributes attribute, ItemEntity[] items)
    {
        OnModuleAttributeChanges changes = new OnModuleAttributeChanges();
        
        foreach (ItemEntity item in items)
            changes.AddChange(this.PrepareNotifyAttributeChange(attribute, item));

        return changes;
    }

    private OnModuleAttributeChanges PrepareNotifyAttributeChange(Attribute[] attributes, ItemEntity item)
    {
        OnModuleAttributeChanges changes = new OnModuleAttributeChanges();

        foreach (Attribute attribute in attributes)
            changes.AddChange(new OnModuleAttributeChange(item, attribute));

        return changes;
    }

    private OnModuleAttributeChanges PrepareNotifyAttributeChange(Attribute[] attributes, ItemEntity[] items)
    {
        if (attributes.Length != items.Length)
            throw new ArgumentOutOfRangeException(nameof(attributes),
                "attributes list and items list must have the same amount of elements");
        
        OnModuleAttributeChanges changes = new OnModuleAttributeChanges();
        
        for(int i = 0; i < attributes.Length; i ++)
            changes.AddChange(new OnModuleAttributeChange(items[i], attributes[i]));

        return changes;
    }

    /// <summary>
    /// Queues a multievent to be sent on the next dogma tick
    /// </summary>
    /// <param name="key">The character to notify</param>
    /// <param name="notif">The notification</param>
    private void QueueMultiEvent(PyTuple key, PyTuple notif)
    {
        lock (this.mPendingEvents)
        {
            // make sure there's an entry for this character
            if (this.mPendingEvents.TryGetValue(key, out PyList<PyTuple> events) == false)
            {
                events = new PyList<PyTuple>();
                this.mPendingEvents.Add(key, events);
            }
        
            // add the notification to the queue
            events.Add(notif);
        }
    }

    /// <summary>
    /// Queues a multievent to be sent to the specified players on the next dogma tick
    /// </summary>
    /// <param name="idType">The type of id to use for notification</param>
    /// <param name="id">The value to notify</param>
    /// <param name="notif">The notification</param>
    public void QueueMultiEvent(string idType, int id, PyTuple notif)
    {
        PyList<PyInteger> ids = new PyList<PyInteger>() {[0] = id};
        
        this.QueueMultiEvent(new PyTuple(2) {[0] = idType, [1] = ids}, notif);
    }

    /// <summary>
    /// Queues a multievent to be sent to the specified players on the next dogma tick
    /// </summary>
    /// <param name="idType">The type of id to use for notification</param>
    /// <param name="id">The value to notify</param>
    /// <param name="notif">The notification</param>
    public void QueueMultiEvent(string idType, PyTuple id, PyTuple notif)
    {
        PyList<PyTuple> ids = new PyList<PyTuple>() {[0] = id};
        
        this.QueueMultiEvent(new PyTuple(2) {[0] = idType, [1] = ids}, notif);
    }

    /// <summary>
    /// Queues a multievent to be sent to the specified players on the next dogma tick
    /// </summary>
    /// <param name="idType"></param>
    /// <param name="ids"></param>
    /// <param name="notif"></param>
    public void QueueMultiEvent(string idType, PyList<PyInteger> ids, PyTuple notif)
    {
        this.QueueMultiEvent(new PyTuple(2) {[0] = idType, [1] = ids}, notif);
    }

    /// <summary>
    /// Queues a multievent to be sent to the specified players on the next dogma tick
    /// </summary>
    /// <param name="idType"></param>
    /// <param name="ids"></param>
    /// <param name="notif"></param>
    public void QueueMultiEvent(string idType, PyList<PyTuple> ids, PyTuple notif)
    {
        this.QueueMultiEvent(new PyTuple(2) {[0] = idType, [1] = ids}, notif);
    }

    /// <summary>
    /// Queues a multievent to be sent to the given ownerID on the next dogma tick
    /// </summary>
    /// <param name="ownerID">The character to notify</param>
    /// <param name="notif">The notification's body</param>
    public void QueueMultiEvent(int ownerID, PyTuple notif)
    {
        this.QueueMultiEvent(NotificationManager.NOTIFICATION_TYPE_OWNER, ownerID, notif);
    }

    /// <summary>
    /// Sends all the queued multievents
    /// </summary>
    private void SendMultiEvents()
    {
        lock (this.mPendingEvents)
        {
            foreach ((PyTuple type, PyList<PyTuple> notifs) in this.mPendingEvents)
            {
                this.NotificationManager.SendNotification(
                    "OnMultiEvent",
                    type[0] as PyString,
                    type[1] as PyList,
                    new PyTuple(1){[0] = notifs}
                );
            }
            
            this.mPendingEvents.Clear();
        }
    }

    private void Tick()
    {
        while (true)
        {
            // for now just do some wait and flush the multi-event queue
            Thread.Sleep(100);
            
            this.SendMultiEvents();
        }
    }

    public Dogma(NotificationManager notificationManager)
    {
        this.NotificationManager = notificationManager;
        this.mThread = new Thread(Tick);
        this.mThread.Start();
    }
}