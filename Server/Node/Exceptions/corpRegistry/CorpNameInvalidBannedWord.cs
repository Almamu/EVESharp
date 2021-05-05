using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.corpRegistry
{
    public class CorpNameInvalidBannedWord : UserError
    {
        public CorpNameInvalidBannedWord() : base("CorpNameInvalidBannedWord")
        {
        }
    }
}