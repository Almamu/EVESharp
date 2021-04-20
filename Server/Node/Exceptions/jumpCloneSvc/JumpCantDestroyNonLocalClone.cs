using EVE.Packets.Exceptions;

namespace Node.Exceptions.jumpCloneSvc
{
    public class JumpCantDestroyNonLocalClone : UserError
    {
        public JumpCantDestroyNonLocalClone() : base("JumpCantDestroyNonLocalClone")
        {
        }
    }
}