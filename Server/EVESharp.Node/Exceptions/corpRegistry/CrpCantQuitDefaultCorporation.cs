using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corpRegistry;

public class CrpCantQuitDefaultCorporation : UserError
{
    public CrpCantQuitDefaultCorporation () : base ("CrpCantQuitDefaultCorporation") { }
}