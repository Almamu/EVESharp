using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.jumpCloneSvc
{
    public class JumpCantDestroyNonLocalClone : UserError
    {
        public JumpCantDestroyNonLocalClone() : base("JumpCantDestroyNonLocalClone")
        {
        }
    }
}