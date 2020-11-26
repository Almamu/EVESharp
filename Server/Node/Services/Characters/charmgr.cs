using Common.Database;
using Node.Database;
using PythonTypes.Types.Primitives;

namespace Node.Services.Characters
{
    public class charmgr : Service
    {
        private readonly CharacterDB mDB = null;
        
        public PyDataType GetPublicInfo(PyInteger characterID, PyDictionary namedPayload, Client client)
        {
            return this.mDB.GetPublicInfo(characterID);
        }
        
        public charmgr(DatabaseConnection db, ServiceManager manager) : base(manager)
        {
            this.mDB = new CharacterDB(db, manager.Container.ItemFactory);
        }
    }
}