using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.jumpCloneSvc
{
    public class JumpCharStoringMaxClonesNone : UserError
    {
        public JumpCharStoringMaxClonesNone() : base("JumpCharStoringMaxClonesNone")
        {
        }
    }
}