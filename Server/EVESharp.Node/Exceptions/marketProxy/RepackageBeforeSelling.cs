using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Exceptions.marketProxy
{
    public class RepackageBeforeSelling : UserError
    {
        public RepackageBeforeSelling(Type type) : base("RepackageBeforeSelling", new PyDictionary {["item"] = FormatTypeIDAsName(type.ID)})
        {
        }
    }
}