using EVE.Packets.Exceptions;

namespace Node.Exceptions.corporationSvc
{
    class MedalNameInvalid : UserError
    {
        public MedalNameInvalid() : base("MedalNameInvalid")
        {
        }
    }
}