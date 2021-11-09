using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.Node.Exceptions.corpRegistry
{
    public class CrpOnlyDirectorsCanProposeVotes : UserError
    {
        public CrpOnlyDirectorsCanProposeVotes() : base("CrpOnlyDirectorsCanProposeVotes")
        {
        }
    }
}