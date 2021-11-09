using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.corpRegistry
{
    public class CanOnlyCreateCorpInStation : UserError
    {
        public CanOnlyCreateCorpInStation() : base("CanOnlyCreateCorpInStation")
        {
        }
    }
}