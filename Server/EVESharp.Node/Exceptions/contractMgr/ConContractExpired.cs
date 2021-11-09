using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.contractMgr
{
    public class ConContractExpired : UserError
    {
        public ConContractExpired() : base("ConContractExpired")
        {
        }
    }
}