using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.character;

internal class CharNameInvalidMinLength : UserError
{
    public CharNameInvalidMinLength () : base ("CharNameInvalidMinLength") { }
}