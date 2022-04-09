using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.jumpCloneSvc;

public class JumpCantDestroyNonLocalClone : UserError
{
    public JumpCantDestroyNonLocalClone () : base ("JumpCantDestroyNonLocalClone") { }
}