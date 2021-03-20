using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions
{
    public class ChtWrongRole : UserError
    {
        public ChtWrongRole(string channel, string missingRoles) :
            base("ChtWrongRole", new PyDictionary {["channel"] = channel, ["missingroles"] = missingRoles})
        {
        }
    }
}