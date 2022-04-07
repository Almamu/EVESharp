using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.character;

internal class CharNameInvalidMaxLength : UserError
{
    public CharNameInvalidMaxLength () : base ("CharNameInvalidMaxLength") { }
}