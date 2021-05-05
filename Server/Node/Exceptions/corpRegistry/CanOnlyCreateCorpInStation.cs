using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.corpRegistry
{
    public class CanOnlyCreateCorpInStation : UserError
    {
        public CanOnlyCreateCorpInStation() : base("CanOnlyCreateCorpInStation")
        {
        }
    }
}