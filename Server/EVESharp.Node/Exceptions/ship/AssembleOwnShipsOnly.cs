using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Exceptions.ship
{
    public class AssembleOwnShipsOnly : UserError
    {
        public AssembleOwnShipsOnly(int ownerID) : base("AssembleOwnShipsOnly", new PyDictionary {["owner"] = FormatOwnerID(ownerID)})
        {
        }
    }
}