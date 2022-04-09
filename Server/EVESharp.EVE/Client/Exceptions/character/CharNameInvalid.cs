using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.character;

public class CharNameInvalid : UserError
{
    public CharNameInvalid () : base ("CharNameInvalid") { }
}