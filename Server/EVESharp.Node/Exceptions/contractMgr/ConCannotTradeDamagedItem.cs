using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Exceptions.contractMgr;

public class ConCannotTradeDamagedItem : UserError
{
    public ConCannotTradeDamagedItem(Type type) : base("ConCannotTradeDamagedItem", new PyDictionary {["example"] = FormatTypeIDAsName(type.ID)})
    {
    }
}