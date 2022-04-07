using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.character;

internal class CharNameInvalid : UserError
{
    public CharNameInvalid () : base ("CharNameInvalid") { }
}