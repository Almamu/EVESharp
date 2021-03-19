using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.contractMgr
{
    public class ConTooManyContractsMax : UserError
    {
        public ConTooManyContractsMax(long maximumContracts) : base("ConTooManyContractsMax", new PyDictionary {["max"] = maximumContracts})
        {
        }
    }
}