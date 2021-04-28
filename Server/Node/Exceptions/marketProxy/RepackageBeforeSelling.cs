using EVE.Packets.Exceptions;
using Node.StaticData.Inventory;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.marketProxy
{
    public class RepackageBeforeSelling : UserError
    {
        public RepackageBeforeSelling(Type type) : base("RepackageBeforeSelling", new PyDictionary {["item"] = FormatTypeIDAsName(type.ID)})
        {
        }
    }
}