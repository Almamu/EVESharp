using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.corpRegistry
{
    public class CorpNameInvalidMaxLength : UserError
    {
        public CorpNameInvalidMaxLength() : base("CorpNameInvalidMaxLength")
        {
        }
    }
}