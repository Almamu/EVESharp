using EVE.Packets.Exceptions;

namespace Node.Exceptions.jumpCloneSvc
{
    public class MktNotOwner : UserError
    {
        public MktNotOwner() : base("MktNotOwner")
        {
        }
    }
}