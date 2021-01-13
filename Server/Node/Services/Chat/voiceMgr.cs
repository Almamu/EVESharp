using Common.Services;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.Chat
{
    public class voiceMgr : Service
    {
        public voiceMgr()
        {
        }

        public PyDataType VoiceEnabled(PyDictionary namedPayload, Client client)
        {
            return 0;
        }
    }
}