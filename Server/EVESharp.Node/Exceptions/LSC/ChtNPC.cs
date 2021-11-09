using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Exceptions
{
    public class ChtNPC : UserError
    {
        public ChtNPC(int characterID) : base("ChtNPC", new PyDictionary {["char"] = FormatOwnerID(characterID)})
        {
        }
    }
}