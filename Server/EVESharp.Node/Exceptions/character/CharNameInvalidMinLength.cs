using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.character;

class CharNameInvalidMinLength : UserError
{
    public CharNameInvalidMinLength() : base("CharNameInvalidMinLength")
    {
    }
}