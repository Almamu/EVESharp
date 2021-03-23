using Common.Services;
using Node.Network;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Services.Tutorial
{
    public class tutorialSvc : IService
    {
        public tutorialSvc()
        {
        }

        public PyDataType GetCharacterTutorialState(CallInformation call)
        {
            return new Rowset(new PyList(4)
            {
                [0] = "characterID",
                [1] = "tutorialID",
                [2] = "pageID",
                [3] = "eventTypeID"
            });
        }

        public PyList GetContextHelp(CallInformation call)
        {
            return new PyList();
        }
    }
}
