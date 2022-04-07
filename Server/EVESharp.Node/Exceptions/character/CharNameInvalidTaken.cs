using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.character;

internal class CharNameInvalidTaken : UserError
{
    public CharNameInvalidTaken () : base ("CharNameInvalidTaken") { }
}