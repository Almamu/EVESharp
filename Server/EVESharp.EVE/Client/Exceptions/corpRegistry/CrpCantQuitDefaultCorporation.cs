using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.corpRegistry;

public class CrpCantQuitDefaultCorporation : UserError
{
    public CrpCantQuitDefaultCorporation () : base ("CrpCantQuitDefaultCorporation") { }
}