using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.character;

public class CharNameInvalidMaxLength : UserError
{
    public CharNameInvalidMaxLength () : base ("CharNameInvalidMaxLength") { }
}