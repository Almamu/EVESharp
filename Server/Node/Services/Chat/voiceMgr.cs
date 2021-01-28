using Common.Services;
using Node.Network;
using PythonTypes.Types.Primitives;

namespace Node.Services.Chat
{
    public class voiceMgr : Service
    {
        public voiceMgr()
        {
        }

        public PyDataType VoiceEnabled(CallInformation call)
        {
            return 0;
        }
    }
}