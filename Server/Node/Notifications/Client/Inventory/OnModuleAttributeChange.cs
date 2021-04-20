using System;
using System.Collections.Generic;
using EVE.Packets.Complex;
using Node.Inventory.Items;
using Node.Inventory.Items.Attributes;
using PythonTypes.Types.Primitives;
using Attribute = Node.Inventory.Items.Attributes.Attribute;

namespace Node.Notifications.Client.Inventory
{
    public class OnModuleAttributeChange : ClientNotification
    {
        private const string NOTIFICATION_NAME = "OnModuleAttributeChange";
        
        public ItemEntity Item { get; }
        public Attribute Attribute { get; }
        
        public OnModuleAttributeChange(ItemEntity item, Attribute attribute) : base(NOTIFICATION_NAME)
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