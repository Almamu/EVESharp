using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Attributes;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Notifications;

public interface IDogmaNotifications
{
    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    void NotifyAttributeChange (int ownerID, Attribute attribute, ItemEntity item);

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    void NotifyAttributeChange (int ownerID, int locationID, Attribute attribute, ItemEntity item);

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    void NotifyAttributeChange (NotificationIdType idType, int value, Attribute attribute, ItemEntity item);

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    void NotifyAttributeChange (NotificationIdType idType, PyTuple value, Attribute attribute, ItemEntity item);

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    void NotifyAttributeChange (NotificationIdType idType, PyList <PyInteger> value, Attribute attribute, ItemEntity item);

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    void NotifyAttributeChange (NotificationIdType idType, PyList <PyTuple> value, Attribute attribute, ItemEntity item);

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    void NotifyAttributeChange (int ownerID, AttributeTypes attribute, ItemEntity item);

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    void NotifyAttributeChange (int ownerID, int locationID, AttributeTypes attribute, ItemEntity item);

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    void NotifyAttributeChange (NotificationIdType idType, int value, AttributeTypes attribute, ItemEntity item);

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    void NotifyAttributeChange (NotificationIdType idType, PyTuple value, AttributeTypes attribute, ItemEntity item);

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    void NotifyAttributeChange (NotificationIdType idType, PyList <PyInteger> value, AttributeTypes attribute, ItemEntity item);

    /// <summary>
    /// Notifies the client of a single attribute change on a specific item
    /// </summary>
    /// <param name="attribute">The attribute to notify about</param>
    /// <param name="item">The item to notify about</param>
    void NotifyAttributeChange (NotificationIdType idType, PyList <PyTuple> value, AttributeTypes attribute, ItemEntity item);

    /// <summary>
    /// Notifies the client of a multiple attribute change on a specific item
    /// </summary>
    /// <param name="attributes">The attributes to notify about</param>
    /// <param name="item">The item to notify about</param>
    void NotifyAttributeChange (int ownerID, AttributeTypes [] attributes, ItemEntity item);

    /// <summary>
    /// Notifies the client of a multiple attribute change on a specific item
    /// </summary>
    /// <param name="attributes">The attributes to notify about</param>
    /// <param name="item">The item to notify about</param>
    void NotifyAttributeChange (int ownerID, int locationID, AttributeTypes [] attributes, ItemEntity item);

    /// <summary>
    /// Notifies the client of a multiple attribute change on a specific item
    /// </summary>
    /// <param name="attributes">The attributes to notify about</param>
    /// <param name="item">The item to notify about</param>
    void NotifyAttributeChange (NotificationIdType idType, int value, AttributeTypes [] attributes, ItemEntity item);

    /// <summary>
    /// Notifies the client of a multiple attribute change on a specific item
    /// </summary>
    /// <param name="attributes">The attributes to notify about</param>
    /// <param name="item">The item to notify about</param>
    void NotifyAttributeChange (NotificationIdType idType, PyTuple value, AttributeTypes [] attributes, ItemEntity item);

    /// <summary>
    /// Notifies the client of a multiple attribute change on a specific item
    /// </summary>
    /// <param name="attributes">The attributes to notify about</param>
    /// <param name="item">The item to notify about</param>
    void NotifyAttributeChange (NotificationIdType idType, PyList <PyInteger> value, AttributeTypes [] attributes, ItemEntity item);

    /// <summary>
    /// Notifies the client of a multiple attribute change on a specific item
    /// </summary>
    /// <param name="attributes">The attributes to notify about</param>
    /// <param name="item">The item to notify about</param>
    void NotifyAttributeChange (NotificationIdType idType, PyList <PyTuple> value, AttributeTypes [] attributes, ItemEntity item);

    /// <summary>
    /// Notifies the client of a multiple attribute change on different items
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="items">The list of items those attributes belong to</param>
    /// <exception cref="System.ArgumentOutOfRangeException">If the list of attributes and items is not the same size</exception>
    void NotifyAttributeChange (int ownerID, Attribute [] attributes, ItemEntity [] items);

    /// <summary>
    /// Notifies the client of a multiple attribute change on different items
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="items">The list of items those attributes belong to</param>
    /// <exception cref="System.ArgumentOutOfRangeException">If the list of attributes and items is not the same size</exception>
    void NotifyAttributeChange (int ownerID, int locationID, Attribute [] attributes, ItemEntity [] items);

    /// <summary>
    /// Notifies the client of a multiple attribute change on different items
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="items">The list of items those attributes belong to</param>
    /// <exception cref="System.ArgumentOutOfRangeException">If the list of attributes and items is not the same size</exception>
    void NotifyAttributeChange (NotificationIdType idType, int value, Attribute [] attributes, ItemEntity [] items);

    /// <summary>
    /// Notifies the client of a multiple attribute change on different items
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="items">The list of items those attributes belong to</param>
    /// <exception cref="System.ArgumentOutOfRangeException">If the list of attributes and items is not the same size</exception>
    void NotifyAttributeChange (NotificationIdType idType, PyTuple value, Attribute [] attributes, ItemEntity [] items);

    /// <summary>
    /// Notifies the client of a multiple attribute change on different items
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="items">The list of items those attributes belong to</param>
    /// <exception cref="System.ArgumentOutOfRangeException">If the list of attributes and items is not the same size</exception>
    void NotifyAttributeChange (NotificationIdType idType, PyList <PyInteger> value, Attribute [] attributes, ItemEntity [] items);

    /// <summary>
    /// Notifies the client of a multiple attribute change on different items
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="items">The list of items those attributes belong to</param>
    /// <exception cref="System.ArgumentOutOfRangeException">If the list of attributes and items is not the same size</exception>
    void NotifyAttributeChange (NotificationIdType idType, PyList <PyTuple> value, Attribute [] attributes, ItemEntity [] items);

    /// <summary>
    /// Notifies the client of a multiple attribute change on an item
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="item">The item these attributes belong to</param>
    void NotifyAttributeChange (int ownerID, Attribute [] attributes, ItemEntity item);

    /// <summary>
    /// Notifies the client of a multiple attribute change on an item
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="item">The item these attributes belong to</param>
    void NotifyAttributeChange (int ownerID, int locationID, Attribute [] attributes, ItemEntity item);

    /// <summary>
    /// Notifies the client of a multiple attribute change on an item
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="item">The item these attributes belong to</param>
    void NotifyAttributeChange (NotificationIdType idType, int value, Attribute [] attributes, ItemEntity item);

    /// <summary>
    /// Notifies the client of a multiple attribute change on an item
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="item">The item these attributes belong to</param>
    void NotifyAttributeChange (NotificationIdType idType, PyTuple value, Attribute [] attributes, ItemEntity item);

    /// <summary>
    /// Notifies the client of a multiple attribute change on an item
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="item">The item these attributes belong to</param>
    void NotifyAttributeChange (NotificationIdType idType, PyList <PyInteger> value, Attribute [] attributes, ItemEntity item);

    /// <summary>
    /// Notifies the client of a multiple attribute change on an item
    /// </summary>
    /// <param name="attributes">The list of attributes that have changed</param>
    /// <param name="item">The item these attributes belong to</param>
    void NotifyAttributeChange (NotificationIdType idType, PyList <PyTuple> value, Attribute [] attributes, ItemEntity item);

    void NotifyAttributeChange (int                ownerID, AttributeTypes     attribute,  ItemEntity []  items);
    void NotifyAttributeChange (int                ownerID, int                locationID, AttributeTypes attribute, ItemEntity [] items);
    void NotifyAttributeChange (NotificationIdType idType,  int                value,      AttributeTypes attribute, ItemEntity [] items);
    void NotifyAttributeChange (NotificationIdType idType,  PyTuple            value,      AttributeTypes attribute, ItemEntity [] items);
    void NotifyAttributeChange (NotificationIdType idType,  PyList <PyInteger> value,      AttributeTypes attribute, ItemEntity [] items);
    void NotifyAttributeChange (NotificationIdType idType,  PyList <PyTuple>   value,      AttributeTypes attribute, ItemEntity [] items);

    /// <summary>
    /// Queues a multievent to be sent to the specified players on the next dogma tick
    /// </summary>
    /// <param name="idType">The type of id to use for notification</param>
    /// <param name="id">The value to notify</param>
    /// <param name="notif">The notification</param>
    void QueueMultiEvent (NotificationIdType idType, int id, PyTuple notif);

    /// <summary>
    /// Queues a multievent to be sent to the specified players on the next dogma tick
    /// </summary>
    /// <param name="idType">The type of id to use for notification</param>
    /// <param name="id">The value to notify</param>
    /// <param name="notif">The notification</param>
    void QueueMultiEvent (NotificationIdType idType, PyTuple id, PyTuple notif);

    /// <summary>
    /// Queues a multievent to be sent to the specified players on the next dogma tick
    /// </summary>
    /// <param name="idType"></param>
    /// <param name="ids"></param>
    /// <param name="notif"></param>
    void QueueMultiEvent (NotificationIdType idType, PyList <PyInteger> ids, PyTuple notif);

    /// <summary>
    /// Queues a multievent to be sent to the specified players on the next dogma tick
    /// </summary>
    /// <param name="idType"></param>
    /// <param name="ids"></param>
    /// <param name="notif"></param>
    void QueueMultiEvent (NotificationIdType idType, PyList <PyTuple> ids, PyTuple notif);

    /// <summary>
    /// Queues a multievent to be sent to the given ownerID on the next dogma tick
    /// </summary>
    /// <param name="ownerID">The character to notify</param>
    /// <param name="notif">The notification's body</param>
    void QueueMultiEvent (int ownerID, PyTuple notif);
}