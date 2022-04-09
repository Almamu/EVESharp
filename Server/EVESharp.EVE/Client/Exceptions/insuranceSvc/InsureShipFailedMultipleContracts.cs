using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.insuranceSvc;

public class InsureShipFailedMultipleContracts : UserError
{
    public InsureShipFailedMultipleContracts () : base ("InsureShipFailedMultipleContracts") { }
}