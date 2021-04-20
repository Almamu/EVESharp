using EVE.Packets.Exceptions;

namespace Node.Exceptions.marketProxy
{
    public class MktInvalidRegion : UserError
    {
        public MktInvalidRegion() : base("MktInvalidRegion")
        {
        }
    }
}