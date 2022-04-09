using System;
using EVESharp.Node.Database;
using EVESharp.Node.Notifications;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Chat;

public class MailManager
{
    private MessagesDB         DB            { get; }
    private NotificationSender Notifications { get; }

    public MailManager (MessagesDB db, NotificationSender notificationSender)
    {
        DB            = db;
        Notifications = notificationSender;
    }

    public void SendMail (int fromID, int destinationID, string subject, string message)
    {
        this.SendMail (fromID, new PyList <PyInteger> (1) {[0] = destinationID}, subject, message);
    }

    public void SendMail (int fromID, PyList <PyInteger> destinationMailboxes, string subject, string message)
    {
        foreach (PyInteger destinationID in destinationMailboxes)
        {
            ulong messageID = DB.StoreMail (destinationID, fromID, subject, message, out string mailboxType);

            // send notification to the destination
            PyTuple notification = new PyTuple (5)
            {
                [0] = destinationMailboxes,
                [1] = messageID,
                [2] = fromID,
                [3] = subject,
                [4] = DateTime.UtcNow.ToFileTimeUtc ()
            };

            // *multicastID are a special broadcast type that allows to notify different users based on things like charid or corpid
            // under the same notification, making things easier for us
            // sadly supporting that is more painful that actually spamming the cluster controller with single corpid or charid type broadcast
            // but supporting multicastIDs would be perfect

            // the list of id's on a *multicastID would be a PyTuple with the type and the id in it, instead of just a list of integers
            // TODO: IMPLEMENT MULTICASTING IN THE CLUSTER
            Notifications.SendNotification ("OnMessage", mailboxType, destinationID, notification);
        }
    }
}