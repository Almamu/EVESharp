using EVE.Packets.Exceptions;

namespace Node.Exceptions.jumpCloneSvc
{
    public class JumpCharStoringMaxClonesNone : UserError
    {
        public JumpCharStoringMaxClonesNone() : base("JumpCharStoringMaxClonesNone")
        {
        }
    }
}