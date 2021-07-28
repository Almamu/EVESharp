using EVE.Packets.Exceptions;

namespace Node.Exceptions.corporationSvc
{
    class MedalDescriptionTooLong : UserError
    {
        public MedalDescriptionTooLong() : base("MedalDescriptionTooLong")
        {
        }
    }
}