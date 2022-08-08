using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.character;

public class CharNameInvalidTaken : UserError
{
    public CharNameInvalidTaken () : base ("CharNameInvalidTaken") { }
}