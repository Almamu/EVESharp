using System;
using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.Node.Inventory.Items;
using EVESharp.PythonTypes.Types.Primitives;
using Attribute = EVESharp.EVE.Inventory.Attributes.Attribute;

namespace EVESharp.Node.Notifications.Client.Inventory;

public class OnModuleAttributeChange : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnModuleAttributeChange";

    public ItemEntity Item      { get; }
    public Attribute  Attribute { get; }

    public OnModuleAttributeChange (ItemEntity item, Attribute attribute) : base (NOTIFICATION_NAME)
    {
        Item      = item;
        Attribute = attribute;
    }

    public override List <PyDataType> GetElements ()
    {
        return new List <PyDataType>
        {
            Item.OwnerID,
            Item.ID,
            Attribute.Info.ID,
            DateTime.UtcNow.ToFileTimeUtc (),
            Attribute, // newValue
            Attribute // this should be oldValue, but the client doesn't check, so who cares
        };
    }
}