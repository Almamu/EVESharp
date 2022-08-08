using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Exceptions.contractMgr;

public class ConCannotTradeContraband : UserError
{
    public ConCannotTradeContraband (Type example) : base ("ConCannotTradeContraband", new PyDictionary {["example"] = FormatTypeIDAsName (example.ID)}) { }
}