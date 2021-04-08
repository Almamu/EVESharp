using System;
using System.Collections.Generic;
using System.Security.Policy;
using Node.Inventory.Items;
using Node.Inventory.Items.Attributes;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Inventory.Notifications
{
    public class OnModuleAttributeChange : PyMultiEventEntry
    {
        private const string NOTIFICATION_NAME = "OnModuleAttributeChange";
        
        public ItemEntity Item { get; }
        public ItemAttribute Attribute { get; }
        
        public OnModuleAttributeChange(ItemEntity item, ItemAttribute attribute) : base(NOTIFICATION_NAME)
        {
            this.Item = item;
            this.Attribute = attribute;
        }

        public override List<PyDataType> GetElements()
        {
            return new List<PyDataType>()
            {
                this.Item.OwnerID,
                this.Item.ID,
                this.Attribute.Info.ID,
                DateTime.UtcNow.ToFileTimeUtc(),
                this.Attribute, // newValue
                this.Attribute // this should be oldValue, but the client doesn't check, so who cares
            };
        }
    }
}