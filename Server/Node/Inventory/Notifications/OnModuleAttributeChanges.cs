using System.Collections.Generic;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Inventory.Notifications
{
    public class OnModuleAttributeChanges : PyMultiEventEntry
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