using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.character;

public class CharNameInvalidMinLength : UserError
{
    public CharNameInvalidMinLength () : base ("CharNameInvalidMinLength") { }
}