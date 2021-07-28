using EVE.Packets.Exceptions;

namespace Node.Exceptions.corporationSvc
{
    class MedalNameTooLong : UserError
    {
        public MedalNameTooLong() : base("MedalNameTooLong")
        {
        }
    }
}