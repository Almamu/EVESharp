using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.character;

public class CharNameInvalidBannedWord : UserError
{
    public CharNameInvalidBannedWord () : base ("CharNameInvalidBannedWord") { }
}