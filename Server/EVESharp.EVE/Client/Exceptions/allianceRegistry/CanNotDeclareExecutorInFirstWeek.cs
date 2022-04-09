using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.EVE.Client.Exceptions.allianceRegistry;

public class CanNotDeclareExecutorInFirstWeek : UserError
{
    public CanNotDeclareExecutorInFirstWeek () : base ("CanNotDeclareExecutorInFirstWeek") { }
}