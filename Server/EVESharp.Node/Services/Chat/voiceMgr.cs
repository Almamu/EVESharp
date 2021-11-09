using EVESharp.Common.Services;
using EVESharp.Node.Network;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Chat
{
    public class voiceMgr : IService
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