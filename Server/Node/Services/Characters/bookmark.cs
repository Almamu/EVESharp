using Common.Database;
using Common.Services;
using Node.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.Characters
{
    public class bookmark : Service
    {
        private BookmarkDB DB { get; }
        
        public bookmark(BookmarkDB db)
        {
            this.DB = db;
        }

        public PyDataType GetBookmarks(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            
            return this.DB.GetBookmarks((int) client.CharacterID);
        }
    }
}