using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions
{
    public class ChtNPC : UserError
    {
        public ChtNPC(int characterID) : base("ChtNPC", new PyDictionary {["char"] = FormatOwnerID(characterID)})
        {
        }
    }
}