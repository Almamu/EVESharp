using EVESharp.EVE.Services;
using EVESharp.Node.Network;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Chat
{
    public class voiceMgr : Service
    {
        public override AccessLevel AccessLevel => AccessLevel.Location;
        public voiceMgr()
        {
        }

        public PyDataType VoiceEnabled(CallInformation call)
        {
            return 0;
        }
    }
}