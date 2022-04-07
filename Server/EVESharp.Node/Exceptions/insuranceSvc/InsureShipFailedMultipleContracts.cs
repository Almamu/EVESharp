using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.insuranceSvc;

public class InsureShipFailedMultipleContracts : UserError
{
    public InsureShipFailedMultipleContracts() : base("InsureShipFailedMultipleContracts")
    {
    }
}