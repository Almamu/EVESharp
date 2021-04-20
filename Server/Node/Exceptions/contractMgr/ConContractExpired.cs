using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.contractMgr
{
    public class ConContractExpired : UserError
    {
        public ConContractExpired() : base("ConContractExpired")
        {
        }
    }
}