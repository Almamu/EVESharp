using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.character;

internal class CharNameInvalidMaxSpaces : UserError
{
    public CharNameInvalidMaxSpaces () : base ("CharNameInvalidMaxSpaces") { }
}