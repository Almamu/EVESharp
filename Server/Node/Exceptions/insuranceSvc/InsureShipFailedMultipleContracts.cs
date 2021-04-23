using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.insuranceSvc
{
    public class InsureShipFailedMultipleContracts : UserError
    {
        public InsureShipFailedMultipleContracts() : base("InsureShipFailedMultipleContracts")
        {
        }
    }
}