using EVE.Packets.Exceptions;
using Node.StaticData.Inventory;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.contractMgr
{
    public class ConCannotTradeContraband : UserError
    {
        public ConCannotTradeContraband(Type example) : base("ConCannotTradeContraband", new PyDictionary {["example"] = FormatTypeIDAsName(example.ID)})
        {
        }
    }
}