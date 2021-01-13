using Common.Database;
using Common.Services;
using Node.Database;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.Characters
{
    public class charmgr : Service
    {
        private CharacterDB DB { get; }
        
        public PyDataType GetPublicInfo(PyInteger characterID, PyDictionary namedPayload, Client client)
        {
            return this.DB.GetPublicInfo(characterID);
        }
        
        public charmgr(CharacterDB db)
        {
            this.DB = db;
        }
    }
}