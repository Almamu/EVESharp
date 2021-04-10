using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;

namespace Node.Exceptions.contractMgr
{
    public class ConReturnItemsMissingNonSingleton : UserError
    {
        public ConReturnItemsMissingNonSingleton(string example, string station) : base("ConReturnItemsMissingNonSingleton", new PyDictionary {["example"] = example, ["station"] = station})
        {
        }
    }
}