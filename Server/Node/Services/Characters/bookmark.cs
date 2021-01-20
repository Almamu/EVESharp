using Common.Database;
using Common.Services;
using Node.Database;
using Node.Network;
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

        public PyDataType GetBookmarks(CallInformation call)
        {
            return this.DB.GetBookmarks(call.Client.EnsureCharacterIsSelected());
        }
    }
}