using EVE.Packets.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.contractMgr
{
    public class ConDestinationSame : UserError
    {
        public ConDestinationSame() : base("ConDestinationSame")
        {
        }
    }
}