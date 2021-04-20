using EVE.Packets.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.contractMgr
{
    public class ConNPCNotAllowed : UserError
    {
        public ConNPCNotAllowed() : base("ConNPCNotAllowed")
        {
        }
    }
}