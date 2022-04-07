using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.character;

internal class CharNameInvalidBannedWord : UserError
{
    public CharNameInvalidBannedWord () : base ("CharNameInvalidBannedWord") { }
}