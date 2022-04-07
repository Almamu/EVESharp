using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.corpRegistry;

public class CorpTickerNameInvalidTaken : UserError
{
    public CorpTickerNameInvalidTaken() : base("CorpTickerNameInvalidTaken")
    {
    }
}