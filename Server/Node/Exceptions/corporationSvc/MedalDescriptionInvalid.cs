using EVE.Packets.Exceptions;

namespace Node.Exceptions.corporationSvc
{
    class MedalDescriptionInvalid : UserError
    {
        public MedalDescriptionInvalid() : base("MedalDescriptionInvalid")
        {
        }
    }
}