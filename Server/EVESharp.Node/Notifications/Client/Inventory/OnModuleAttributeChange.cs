using System;
using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Inventory.Items.Attributes;
using EVESharp.PythonTypes.Types.Primitives;
using Attribute = EVESharp.Node.Inventory.Items.Attributes.Attribute;

namespace EVESharp.Node.Notifications.Client.Inventory;

public class OnModuleAttributeChange : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnModuleAttributeChange";
        
    public ItemEntity                                Item      { get; }
    public Node.Inventory.Items.Attributes.Attribute Attribute { get; }
        
    public OnModuleAttributeChange(ItemEntity item, Node.Inventory.Items.Attributes.Attribute attribute) : base(NOTIFICATION_NAME)
    {
        this.Item      = item;
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