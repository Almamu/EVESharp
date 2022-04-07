using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.character;

class CharNameInvalidMaxLength : UserError
{
    public CharNameInvalidMaxLength() : base("CharNameInvalidMaxLength")
    {
    }
}