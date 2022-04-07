using EVESharp.Common.Database;
using EVESharp.Node.Inventory.Items.Attributes;
using EVESharp.Node.Inventory.Items.Types.Information;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.Node.StaticData;

namespace EVESharp.Node.Inventory.Items.Types;

public class Item : ItemEntity
{
    public Item(Information.Item info) : base(info)
    {
    }
}