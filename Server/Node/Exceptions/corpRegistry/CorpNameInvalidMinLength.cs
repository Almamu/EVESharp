using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.corpRegistry
{
    public class CorpNameInvalidMinLength : UserError
    {
        public CorpNameInvalidMinLength() : base("CorpNameInvalidMinLength")
        {
        }
    }
}