using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.contractMgr
{
    public class ConMinMaxPriceError : UserError
    {
        public ConMinMaxPriceError() : base("ConMinMaxPriceError")
        {
        }
    }
}