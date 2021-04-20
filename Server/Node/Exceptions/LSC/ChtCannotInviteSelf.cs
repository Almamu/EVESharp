using EVE.Packets.Exceptions;

namespace Node.Exceptions
{
    public class ChtCannotInviteSelf : UserError
    {
        public ChtCannotInviteSelf() : base("ChtCannotInviteSelf")
        {
        }
    }
}