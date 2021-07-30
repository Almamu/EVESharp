using EVE.Packets.Exceptions;
using PythonTypes.Types.Collections;

namespace Node.Exceptions.corpRegistry
{
    public class CrpOnlyDirectorsCanProposeVotes : UserError
    {
        public CrpOnlyDirectorsCanProposeVotes() : base("CrpOnlyDirectorsCanProposeVotes")
        {
        }
    }
}