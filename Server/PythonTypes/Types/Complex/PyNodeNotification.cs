using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Complex
{
    /// <summary>
    /// Special class that handles node notifications only
    /// </summary>
    public abstract class PyNodeNotification
    {
        public string NotificationName { get; init; }

        public PyNodeNotification(string name)
        {
            this.NotificationName = name;
        }

        protected abstract PyDataType GetNotification();
        
        public static implicit operator PyTuple(PyNodeNotification notif)
        {
            return new PyTuple(2)
            {
                [0] = notif.NotificationName,
                [1] = notif.GetNotification()
            };
        }
    }
}