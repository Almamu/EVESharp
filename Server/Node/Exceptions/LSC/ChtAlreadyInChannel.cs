using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions
{
    public class ChtAlreadyInChannel : UserError
    {
        public ChtAlreadyInChannel(int characterID) : base("ChtAlreadyInChannel", new PyDictionary {["char"] = FormatOwnerID(characterID)})
        {
        }
    }
}