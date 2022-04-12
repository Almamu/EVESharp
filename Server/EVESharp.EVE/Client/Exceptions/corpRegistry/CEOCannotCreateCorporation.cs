using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.corpRegistry;

public class CEOCannotCreateCorporation : UserError
{
    public CEOCannotCreateCorporation () : base ("CEOCannotCreateCorporation") { }
}