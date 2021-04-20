using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.contractMgr
{
    public class ConMinMaxPriceError : UserError
    {
        public ConMinMaxPriceError() : base("ConMinMaxPriceError")
        {
        }
    }
}