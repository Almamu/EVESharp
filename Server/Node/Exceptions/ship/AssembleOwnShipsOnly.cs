using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.ship
{
    public class AssembleOwnShipsOnly : UserError
    {
        public AssembleOwnShipsOnly(int ownerID) : base("AssembleOwnShipsOnly", new PyDictionary {["owner"] = FormatOwnerID(ownerID)})
        {
        }
    }
}