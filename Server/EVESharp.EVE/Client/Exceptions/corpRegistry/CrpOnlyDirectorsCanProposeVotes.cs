using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.corpRegistry;

public class CrpOnlyDirectorsCanProposeVotes : UserError
{
    public CrpOnlyDirectorsCanProposeVotes () : base ("CrpOnlyDirectorsCanProposeVotes") { }
}