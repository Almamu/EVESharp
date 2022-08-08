using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Exceptions.corpRegistry;

public class CrpOnlyDirectorsCanProposeVotes : UserError
{
    public CrpOnlyDirectorsCanProposeVotes () : base ("CrpOnlyDirectorsCanProposeVotes") { }
}