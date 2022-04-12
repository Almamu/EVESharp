using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.character;

public class CharNameInvalidMinLength : UserError
{
    public CharNameInvalidMinLength () : base ("CharNameInvalidMinLength") { }
}