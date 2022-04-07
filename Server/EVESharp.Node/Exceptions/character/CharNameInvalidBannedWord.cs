using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.character;

class CharNameInvalidBannedWord : UserError
{
    public CharNameInvalidBannedWord() : base("CharNameInvalidBannedWord")
    {
    }
}