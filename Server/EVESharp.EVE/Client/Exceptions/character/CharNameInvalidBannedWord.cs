using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.character;

public class CharNameInvalidBannedWord : UserError
{
    public CharNameInvalidBannedWord () : base ("CharNameInvalidBannedWord") { }
}