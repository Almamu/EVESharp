using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.StaticData;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.marketProxy;

public class RepairBeforeSelling : UserError
{
    public RepairBeforeSelling(Type type) : base("RepairBeforeSelling", new PyDictionary {["item"] = FormatTypeIDAsName(type.ID), ["otheritem"] = FormatTypeIDAsName(type.ID)})
    {
    }
}