using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions
{
    public class ChtCharNotReachable : UserError
    {
        public ChtCharNotReachable(int characterID) : base("ChtCharNotReachable", new PyDictionary {["char"] = FormatOwnerID(characterID)})
        {
        }
    }
}