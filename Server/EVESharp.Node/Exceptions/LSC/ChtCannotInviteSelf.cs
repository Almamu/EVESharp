using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.LSC;

public class ChtCannotInviteSelf : UserError
{
    public ChtCannotInviteSelf () : base ("ChtCannotInviteSelf") { }
}