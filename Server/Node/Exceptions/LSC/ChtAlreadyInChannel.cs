using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions
{
    public class ChtAlreadyInChannel : UserError
    {
        public ChtAlreadyInChannel(string charName) : base("ChtAlreadyInChannel", new PyDictionary {["char"] = charName})
        {
        }
    }
}