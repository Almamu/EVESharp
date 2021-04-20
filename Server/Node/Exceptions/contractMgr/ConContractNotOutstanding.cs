using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.contractMgr
{
    public class ConContractNotOutstanding : UserError
    {
        public ConContractNotOutstanding() : base("ConContractNotOutstanding")
        {
        }
    }
}