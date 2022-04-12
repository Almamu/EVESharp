using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Client.Exceptions.marketProxy;

public class RepairBeforeSelling : UserError
{
    public RepairBeforeSelling (Type type) : base (
        "RepairBeforeSelling", new PyDictionary
        {
            ["item"]      = FormatTypeIDAsName (type.ID),
            ["otheritem"] = FormatTypeIDAsName (type.ID)
        }
    ) { }
}