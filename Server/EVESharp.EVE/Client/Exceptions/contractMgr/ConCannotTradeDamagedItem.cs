using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Client.Exceptions.contractMgr;

public class ConCannotTradeDamagedItem : UserError
{
    public ConCannotTradeDamagedItem (Type type) : base ("ConCannotTradeDamagedItem", new PyDictionary {["example"] = FormatTypeIDAsName (type.ID)}) { }
}