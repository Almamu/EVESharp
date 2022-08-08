using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.corpRegistry;

public class CrpCantQuitDefaultCorporation : UserError
{
    public CrpCantQuitDefaultCorporation () : base ("CrpCantQuitDefaultCorporation") { }
}