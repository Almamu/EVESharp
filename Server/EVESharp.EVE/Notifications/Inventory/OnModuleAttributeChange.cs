using System;
using System.Collections.Generic;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Packets.Complex;
using EVESharp.Types;
using Attribute = EVESharp.EVE.Data.Inventory.Attributes.Attribute;

namespace EVESharp.EVE.Notifications.Inventory;

public class OnModuleAttributeChange : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnModuleAttributeChange";

    public ItemEntity Item      { get; }
    public Attribute  Attribute { get; }

    public OnModuleAttributeChange (ItemEntity item, Attribute attribute) : base (NOTIFICATION_NAME)
    {
        this.Item      = item;
        this.Attribute = attribute;
    }

    public override List <PyDataType> GetElements ()
    {
        return new List <PyDataType>
        {
            this.Item.OwnerID,
            this.Item.ID,
            this.Attribute.ID,
            DateTime.UtcNow.ToFileTimeUtc (),
            this.Attribute, // newValue
            this.Attribute // this should be oldValue, but the client doesn't check, so who cares
        };
    }
}