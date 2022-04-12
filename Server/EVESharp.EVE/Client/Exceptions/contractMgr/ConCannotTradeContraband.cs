using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Client.Exceptions.contractMgr;

public class ConCannotTradeContraband : UserError
{
    public ConCannotTradeContraband (Type example) : base ("ConCannotTradeContraband", new PyDictionary {["example"] = FormatTypeIDAsName (example.ID)}) { }
}