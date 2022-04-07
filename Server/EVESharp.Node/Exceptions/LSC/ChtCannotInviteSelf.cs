using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions;

public class ChtCannotInviteSelf : UserError
{
    public ChtCannotInviteSelf() : base("ChtCannotInviteSelf")
    {
    }
}