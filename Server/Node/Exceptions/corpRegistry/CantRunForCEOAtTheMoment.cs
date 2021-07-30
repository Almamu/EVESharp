using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.corpRegistry
{
    public class CantRunForCEOAtTheMoment : UserError
    {
        public CantRunForCEOAtTheMoment() : base("CantRunForCEOAtTheMoment")
        {
        }
    }
}