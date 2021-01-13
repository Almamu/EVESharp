using Common.Services;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.Network
{
    public class clientStatsMgr : Service
    {
        public clientStatsMgr()
        {
        }

        public PyDataType SubmitStats(PyTuple stats, PyDictionary namedPayload, Client client)
        {
            // this data is useless as we don't develop the game
            // it seems to contain memory usage, OS information, ping, etc
            return null;
        }
    }
}