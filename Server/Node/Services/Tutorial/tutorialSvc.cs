using Common.Services;
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

        public PyDataType GetCharacterTutorialState(PyDictionary namedPayload, Client client)
        {
            return new Rowset(new PyDataType []
            {
                "characterID", "tutorialID", "pageID", "eventTypeID"
            });
        }
    }
}