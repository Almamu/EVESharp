using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corpRegistry;

public class CEOCannotCreateCorporation : UserError
{
    public CEOCannotCreateCorporation () : base ("CEOCannotCreateCorporation") { }
}