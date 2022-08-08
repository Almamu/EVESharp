using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.LSC;

public class ChtCannotInviteSelf : UserError
{
    public ChtCannotInviteSelf () : base ("ChtCannotInviteSelf") { }
}