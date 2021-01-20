using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions
{
    public class ChtCharNotReachable : UserError
    {
        public ChtCharNotReachable(string charName) : base("ChtCharNotReachable", new PyDictionary {["char"] = charName})
        {
        }
    }
}