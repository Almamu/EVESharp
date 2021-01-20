using Common.Services;
using Node.Network;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.Network
{
    public class clientStatsMgr : Service
    {
        public clientStatsMgr()
        {
        }

        public PyDataType SubmitStats(PyTuple stats, CallInformation call)
        {
            // this data is useless as we don't develop the game
            // it seems to contain memory usage, OS information, ping, etc
            return null;
        }
    }
}