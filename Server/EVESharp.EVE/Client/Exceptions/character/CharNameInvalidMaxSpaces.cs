using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.character;

public class CharNameInvalidMaxSpaces : UserError
{
    public CharNameInvalidMaxSpaces () : base ("CharNameInvalidMaxSpaces") { }
}