using System.Collections.Generic;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Packets.Complex;

/// <summary>
/// This is not a real type in EVE, but should help construct notifications in an easier way 
/// </summary>
public abstract class ClientNotification
{
    /// <summary>
    /// The name of this notification
    /// </summary>
    public string NotificationName { get; }
        
    protected ClientNotification(string notificationName)
    {
        NotificationName = notificationName;
    }

    /// <summary>
    /// Generates the PyDataType elements that the notification should send
    /// </summary>
    /// <returns>The list of items to add after the notification type</returns>
    public abstract List<PyDataType> GetElements();

    public static implicit operator PyTuple(ClientNotification notification)
    {
        List<PyDataType> data = notification.GetElements();
            
        PyTuple result = new PyTuple(1 + (data?.Count ?? 0))
        {
            [0] = notification.NotificationName
        };

        int i = 1;

        // add the rest of the data to the notification
        if (data is not null)
            foreach (PyDataType entry in data)
                result[i++] = entry;

        return result;
    }
}