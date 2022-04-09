using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.character;

public class CharNameInvalidTaken : UserError
{
    public CharNameInvalidTaken () : base ("CharNameInvalidTaken") { }
}