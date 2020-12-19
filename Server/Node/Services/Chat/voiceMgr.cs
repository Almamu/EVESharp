using PythonTypes.Types.Primitives;

namespace Node.Services.Chat
{
    public class voiceMgr : Service
    {
        public voiceMgr(ServiceManager manager) : base(manager)
        {
        }

        public PyDataType VoiceEnabled(PyDictionary namedPayload, Client client)
        {
            return 0;
        }
    }
}