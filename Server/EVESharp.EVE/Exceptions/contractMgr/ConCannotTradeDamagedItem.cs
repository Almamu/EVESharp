using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Exceptions.contractMgr;

public class ConCannotTradeDamagedItem : UserError
{
    public ConCannotTradeDamagedItem (Type type) : base ("ConCannotTradeDamagedItem", new PyDictionary {["example"] = FormatTypeIDAsName (type.ID)}) { }
}