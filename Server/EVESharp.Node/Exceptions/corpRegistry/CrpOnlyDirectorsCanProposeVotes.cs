using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corpRegistry;

public class CrpOnlyDirectorsCanProposeVotes : UserError
{
    public CrpOnlyDirectorsCanProposeVotes () : base ("CrpOnlyDirectorsCanProposeVotes") { }
}