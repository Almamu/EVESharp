using EVESharp.Database.Inventory.Types;
using EVESharp.EVE.Data.Inventory;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Exceptions.contractMgr;

public class ConCannotTradeContraband : UserError
{
    public ConCannotTradeContraband (Type example) : base ("ConCannotTradeContraband", new PyDictionary {["example"] = FormatTypeIDAsName (example.ID)}) { }
}