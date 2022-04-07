using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.allianceRegistry;

public class CanNotDeclareExecutorInFirstWeek : UserError
{
    public CanNotDeclareExecutorInFirstWeek () : base ("CanNotDeclareExecutorInFirstWeek") { }
}