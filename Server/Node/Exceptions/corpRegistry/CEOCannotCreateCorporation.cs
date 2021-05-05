using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.corpRegistry
{
    public class CEOCannotCreateCorporation : UserError
    {
        public CEOCannotCreateCorporation() : base("CEOCannotCreateCorporation")
        {
        }
    }
}