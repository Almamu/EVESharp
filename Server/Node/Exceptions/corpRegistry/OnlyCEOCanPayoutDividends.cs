using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.corpRegistry
{
    public class OnlyCEOCanPayoutDividends : UserError
    {
        public OnlyCEOCanPayoutDividends() : base("OnlyCEOCanPayoutDividends")
        {
        }
    }
}