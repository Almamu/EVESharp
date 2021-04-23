using System;
using Node.Database;
using Node.Network;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Chat
{
    public class MailManager
    {
        private MessagesDB DB { get; init; }
        private NotificationManager NotificationManager { get; init; }

        public MailManager(MessagesDB db, NotificationManager notificationManager)
        {
            this.DB = db;
            this.NotificationManager = notificationManager;
        }

        public void SendMail(int fromID, int destinationID, string subject, string message)
        {
            this.SendMail(fromID, new PyList<PyInteger>(1) {[0] = destinationID}, subject, message);
        }

        public void SendMail(int fromID, PyList<PyInteger> destinationMailboxes, string subject, string message)
        {
            foreach (PyInteger destinationID in destinationMailboxes)
            {
                ulong messageID = this.DB.StoreMail(destinationID, fromID, subject, message, out string mailboxType);
                
                // send notification to the destination
                PyTuple notification = new PyTuple(5)
                {
                    [0] = destinationMailboxes,
                    [1] = messageID,
                    [2] = fromID,
                    [3] = subject,
                    [4] = DateTime.UtcNow.ToFileTimeUtc()
                };
                
                // *multicastID are a special broadcast type that allows to notify different users based on things like charid or corpid
                // under the same notification, making things easier for us
                // sadly supporting that is more painful that actually spamming the cluster controller with single corpid or charid type broadcast
                // but supporting multicastIDs would be perfect
                
                // the list of id's on a *multicastID would be a PyTuple with the type and the id in it, instead of just a list of integers
                // TODO: IMPLEMENT MULTICASTING IN THE CLUSTER
                this.NotificationManager.SendNotification("OnMessage", mailboxType, destinationID, notification);
            }
        }
    }
}