using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.character;

public class CharNameInvalidMaxSpaces : UserError
{
    public CharNameInvalidMaxSpaces () : base ("CharNameInvalidMaxSpaces") { }
}