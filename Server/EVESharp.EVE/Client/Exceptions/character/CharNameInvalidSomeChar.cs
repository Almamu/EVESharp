using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.character;

public class CharNameInvalidSomeChar : UserError
{
    public CharNameInvalidSomeChar () : base ("CharNameInvalidSomeChar") { }
}