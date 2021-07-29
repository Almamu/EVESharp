using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.corpRegistry
{
    public class CrpCantQuitDefaultCorporation : UserError
    {
        public CrpCantQuitDefaultCorporation() : base("CrpCantQuitDefaultCorporation")
        {
        }
    }
}