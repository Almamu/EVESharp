using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.insuranceSvc;

public class InsureShipFailedMultipleContracts : UserError
{
    public InsureShipFailedMultipleContracts () : base ("InsureShipFailedMultipleContracts") { }
}