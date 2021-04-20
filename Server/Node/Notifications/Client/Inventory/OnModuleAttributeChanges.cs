using System.Collections.Generic;
using EVE.Packets.Complex;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Client.Inventory
{
    public class OnModuleAttributeChanges : ClientNotification
    {
        private const string NOTITIFATION_NAME = "OnModuleAttributeChanges";
        
        public PyList Changes { get; }
        
        public OnModuleAttributeChanges() : base(NOTITIFATION_NAME)
        {
            this.Changes = new PyList();
        }

        public void AddChange(OnModuleAttributeChange change)
        {
            this.Changes.Add(change);
        }
        
        public override List<PyDataType> GetElements()
        {
            return new List<PyDataType>()
            {
                this.Changes
            };
        }
    }
}