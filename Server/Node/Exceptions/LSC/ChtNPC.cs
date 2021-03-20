using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions
{
    public class ChtNPC : UserError
    {
        public ChtNPC(string charName) : base("ChtNPC", new PyDictionary {["char"] = charName})
        {
        }
    }
}