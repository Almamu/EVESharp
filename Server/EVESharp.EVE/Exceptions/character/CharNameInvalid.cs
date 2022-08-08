using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.character;

public class CharNameInvalid : UserError
{
    public CharNameInvalid () : base ("CharNameInvalid") { }
}