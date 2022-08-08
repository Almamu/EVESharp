using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.character;

public class CharNameInvalidMaxLength : UserError
{
    public CharNameInvalidMaxLength () : base ("CharNameInvalidMaxLength") { }
}