using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.corpRegistry
{
    public class CorpNameInvalidTaken : UserError
    {
        public CorpNameInvalidTaken() : base("CorpNameInvalidTaken")
        {
        }
    }
}