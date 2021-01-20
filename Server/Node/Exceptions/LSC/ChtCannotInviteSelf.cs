using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Exceptions
{
    public class ChtCannotInviteSelf : UserError
    {
        public ChtCannotInviteSelf() : base("ChtCannotInviteSelf")
        {
        }
    }
}