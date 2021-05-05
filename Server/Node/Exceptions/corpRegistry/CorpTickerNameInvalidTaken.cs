using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.corpRegistry
{
    public class CorpTickerNameInvalidTaken : UserError
    {
        public CorpTickerNameInvalidTaken() : base("CorpTickerNameInvalidTaken")
        {
        }
    }
}