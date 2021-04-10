using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;

namespace Node.Exceptions.contractMgr
{
    public class ConContractExpired : UserError
    {
        public ConContractExpired() : base("ConContractExpired")
        {
        }
    }
}