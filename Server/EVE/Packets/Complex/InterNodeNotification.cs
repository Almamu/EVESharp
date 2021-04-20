using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace EVE.Packets.Complex
{
    /// <summary>
    /// Special class that handles node notifications only
    /// </summary>
    public abstract class InterNodeNotification
    {
        public string NotificationName { get; init; }

        public InterNodeNotification(string name)
        {
            this.NotificationName = name;
        }

        protected abstract PyDataType GetNotification();
        
        public static implicit operator PyTuple(InterNodeNotification notif)
        {
            return new PyTuple(2)
            {
                [0] = notif.NotificationName,
                [1] = notif.GetNotification()
            };
        }
    }
}