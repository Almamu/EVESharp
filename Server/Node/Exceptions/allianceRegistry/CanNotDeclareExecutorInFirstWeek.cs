using EVE.Packets.Exceptions;

namespace Node.Exceptions.allianceRegistry
{
    public class CanNotDeclareExecutorInFirstWeek : UserError
    {
        public CanNotDeclareExecutorInFirstWeek() : base("CanNotDeclareExecutorInFirstWeek")
        {
            
        }
    }
}