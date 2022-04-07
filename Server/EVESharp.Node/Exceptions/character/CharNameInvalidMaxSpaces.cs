using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.character;

class CharNameInvalidMaxSpaces : UserError
{
    public CharNameInvalidMaxSpaces() : base("CharNameInvalidMaxSpaces")
    {
    }
}