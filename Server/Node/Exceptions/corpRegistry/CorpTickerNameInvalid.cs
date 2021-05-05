using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.corpRegistry
{
    public class CorpTickerNameInvalid : UserError
    {
        public CorpTickerNameInvalid() : base("CorpTickerNameInvalid")
        {
        }
    }
}