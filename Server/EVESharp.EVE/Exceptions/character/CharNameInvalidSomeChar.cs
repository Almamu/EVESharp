using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.character;

public class CharNameInvalidSomeChar : UserError
{
    public CharNameInvalidSomeChar () : base ("CharNameInvalidSomeChar") { }
}