using Common.Database;
using Node.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Characters
{
    public class bookmark : Service
    {
        private BookmarkDB mDB = null;
        
        public bookmark(DatabaseConnection db, ServiceManager manager) : base(manager)
        {
            this.mDB = new BookmarkDB(db);
        }

        public PyDataType GetBookmarks(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            
            return this.mDB.GetBookmarks((int) client.CharacterID);
        }
    }
}