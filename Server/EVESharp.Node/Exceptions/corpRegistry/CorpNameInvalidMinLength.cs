using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.corpRegistry;

public class CorpNameInvalidMinLength : UserError
{
    public CorpNameInvalidMinLength() : base("CorpNameInvalidMinLength")
    {
    }
}