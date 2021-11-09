using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corporationSvc
{
    class MedalDescriptionTooLong : UserError
    {
        public MedalDescriptionTooLong() : base("MedalDescriptionTooLong")
        {
        }
    }
}