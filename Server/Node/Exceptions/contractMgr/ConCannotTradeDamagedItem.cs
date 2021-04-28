using EVE.Packets.Exceptions;
using Node.StaticData.Inventory;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.contractMgr
{
    public class ConCannotTradeDamagedItem : UserError
    {
        public ConCannotTradeDamagedItem(Type type) : base("ConCannotTradeDamagedItem", new PyDictionary {["example"] = FormatTypeIDAsName(type.ID)})
        {
        }
    }
}