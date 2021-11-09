using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Exceptions.contractMgr
{
    public class ConTooManyContractsMax : UserError
    {
        public ConTooManyContractsMax(long maximumContracts) : base("ConTooManyContractsMax", new PyDictionary {["max"] = FormatAmount(maximumContracts)})
        {
        }
    }
}