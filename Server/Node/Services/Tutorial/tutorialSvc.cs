using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Services.Tutorial
{
    public class tutorialSvc : Service
    {
        public tutorialSvc(ServiceManager manager) : base(manager)
        {
        }

        public PyDataType GetCharacterTutorialState(PyDictionary namedPayload, Client client)
        {
            return new Rowset(new string []
            {
                "characterID", "tutorialID", "pageID", "eventTypeID"
            });
        }
    }
}