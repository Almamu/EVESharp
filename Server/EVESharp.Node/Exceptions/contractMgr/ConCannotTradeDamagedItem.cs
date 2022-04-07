using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.contractMgr;

public class ConCannotTradeDamagedItem : UserError
{
    public ConCannotTradeDamagedItem (Type type) : base ("ConCannotTradeDamagedItem", new PyDictionary {["example"] = FormatTypeIDAsName (type.ID)}) { }
}