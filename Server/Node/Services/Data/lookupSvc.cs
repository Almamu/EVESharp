using Common.Services;
using Node.Database;
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

        public PyDataType LookupPlayerCharacters(PyString criteria, PyDataType exactMatch, CallInformation call)
        {
            bool exact = false;

            if (exactMatch is PyBool exactBool)
                exact = exactBool;
            else if (exactMatch is PyInteger exactInteger)
                exact = exactInteger != 0;
            
            return this.DB.LookupPlayerCharacters(criteria, exact);
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

        public PyDataType LookupCharacters(PyString criteria, PyDataType exactMatch, CallInformation call)
        {
            bool exact = false;

            if (exactMatch is PyBool exactBool)
                exact = exactBool;
            else if (exactMatch is PyInteger exactInteger)
                exact = exactInteger != 0;
            
            return this.DB.LookupCharacters(criteria, exact);
        }

        public PyDataType LookupStations(PyString criteria, PyDataType exactMatch, CallInformation call)
        {
            bool exact = false;

            if (exactMatch is PyBool exactBool)
                exact = exactBool;
            else if (exactMatch is PyInteger exactInteger)
                exact = exactInteger != 0;
            
            return this.DB.LookupStations(criteria, exact);
        }
    }
}