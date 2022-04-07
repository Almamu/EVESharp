using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.character;

internal class CharNameInvalidSomeChar : UserError
{
    public CharNameInvalidSomeChar () : base ("CharNameInvalidSomeChar") { }
}