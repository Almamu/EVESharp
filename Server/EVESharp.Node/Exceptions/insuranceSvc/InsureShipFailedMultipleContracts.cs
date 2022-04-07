using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.insuranceSvc;

public class InsureShipFailedMultipleContracts : UserError
{
    public InsureShipFailedMultipleContracts () : base ("InsureShipFailedMultipleContracts") { }
}