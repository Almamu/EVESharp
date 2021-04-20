using EVE.Packets.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions.marketProxy
{
    public class MktOrderDidNotMatch : UserError
    {
        public MktOrderDidNotMatch() : base("MktOrderDidNotMatch")
        {
        }
    }
}