using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.jumpCloneSvc;

public class JumpCharStoringMaxClonesNone : UserError
{
    public JumpCharStoringMaxClonesNone () : base ("JumpCharStoringMaxClonesNone") { }
}