using Common.Services;
using Node.Network;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.Tutorial
{
    public class tutorialSvc : Service
    {
        public tutorialSvc()
        {
        }

        public PyDataType GetCharacterTutorialState(CallInformation call)
        {
            return new Rowset(new PyDataType []
            {
                "characterID", "tutorialID", "pageID", "eventTypeID"
            });
        }
    }
}