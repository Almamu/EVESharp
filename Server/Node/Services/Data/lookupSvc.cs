using Common.Services;
using Node.Network;
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

        public PyDataType LookupPlayerCharacters(PyString criteria, PyInteger exactMatch, CallInformation call)
        {
            return this.LookupPlayerCharacters(criteria, exactMatch == 1);
        }

        public PyDataType LookupPlayerCharacters(PyString criteria, PyBool exactMatch, CallInformation call)
        {
            return this.LookupPlayerCharacters(criteria, exactMatch);
        }
        
        private PyDataType LookupPlayerCharacters(string criteria, bool exactMatch)
        {
            return this.DB.LookupPlayerCharacters(criteria, exactMatch);
        }

        public PyDataType LookupOwners(PyString criteria, PyDataType exactMatch, CallInformation call)
        {
            bool exact = false;

            if (exactMatch is PyBool exactBool)
                exact = exactBool;
            else if (exactMatch is PyInteger exactInteger)
                exact = exactInteger != 0;
            
            return this.DB.LookupOwners(criteria, exact);
        }
    }
}