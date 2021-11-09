using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Exceptions.corporationSvc
{
    class MedalDescriptionInvalid : UserError
    {
        public MedalDescriptionInvalid() : base("MedalDescriptionInvalid")
        {
        }
    }
}