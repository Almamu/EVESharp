using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Nodes
{
    /// <summary>
    /// Special class that handles node notifications only
    /// </summary>
    public abstract class NodeNotification
    {
        public string NotificationName { get; init; }

        public NodeNotification(string name)
        {
            this.NotificationName = name;
        }

        protected abstract PyDataType GetNotification();
        
        public static implicit operator PyTuple(NodeNotification notif)
        {
            return new PyTuple(2)
            {
                [0] = notif.NotificationName,
                [1] = notif.GetNotification()
            };
        }
    }
}