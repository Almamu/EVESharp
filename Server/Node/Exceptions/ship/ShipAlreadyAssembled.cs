using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.ship
{
    public class ShipAlreadyAssembled : UserError
    {
        public ShipAlreadyAssembled(string type) : base("ShipAlreadyAssembled", new PyDictionary { ["type"] = type })
        {
        }
    }
}