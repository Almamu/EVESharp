using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Complex
{
    /// <summary>
    /// This is not a real type in EVE, but should help construct notifications in an easier way 
    /// </summary>
    public abstract class PyMultiEventEntry
    {
        /// <summary>
        /// The name of this notification
        /// </summary>
        public string NotificationName { get; }
        
        protected PyMultiEventEntry(string notificationName)
        {
            this.NotificationName = notificationName;
        }

        /// <summary>
        /// Generates the PyDataType elements that the notification should send
        /// </summary>
        /// <returns>The list of items to add after the notification type</returns>
        protected abstract List<PyDataType> GetElements();

        public static implicit operator PyDataType(PyMultiEventEntry multiEventEntry)
        {
            List<PyDataType> data = multiEventEntry.GetElements();
            
            PyTuple result = new PyTuple(1 + (data?.Count ?? 0))
            {
                [0] = multiEventEntry.NotificationName
            };

            int i = 1;

            // add the rest of the data to the notification
            foreach (PyDataType entry in data)
                result[i++] = entry;

            return result;
        }
    }
}