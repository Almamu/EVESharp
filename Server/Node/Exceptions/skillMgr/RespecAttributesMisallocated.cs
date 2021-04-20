using EVE.Packets.Exceptions;

namespace Node.Exceptions.skillMgr
{
    public class RespecAttributesMisallocated : UserError
    {
        public RespecAttributesMisallocated() : base("RespecAttributesMisallocated")
        {
        }
    }
}