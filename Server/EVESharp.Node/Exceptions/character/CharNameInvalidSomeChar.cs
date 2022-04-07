using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.character;

class CharNameInvalidSomeChar : UserError
{
    public CharNameInvalidSomeChar() : base("CharNameInvalidSomeChar")
    {
    }
}