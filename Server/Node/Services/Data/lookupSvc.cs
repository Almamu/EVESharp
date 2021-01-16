using Common.Services;
using PythonTypes.Types.Primitives;

namespace Node.Services.Data
{
    public class lookupSvc : Service
    {
        private LookupDB DB { get; }
        
        public lookupSvc(LookupDB db)
        {
            this.DB = db;
        }

        public PyDataType LookupPlayerCharacters(PyString criteria, PyInteger exactMatch, PyDictionary namedPayload, Client client)
        {
            return this.LookupPlayerCharacters(criteria, exactMatch == 1);
        }

        public PyDataType LookupPlayerCharacters(PyString criteria, PyBool exactMatch, PyDictionary namedPayload, Client client)
        {
            return this.LookupPlayerCharacters(criteria, exactMatch);
        }
        
        private PyDataType LookupPlayerCharacters(string criteria, bool exactMatch)
        {
            return this.DB.LookupPlayerCharacters(criteria, exactMatch);
        }
    }
}