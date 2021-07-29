using Common.Services;
using Node.Database;
using Node.Network;
using Node.StaticData.Inventory;
using PythonTypes.Types.Primitives;

namespace Node.Services.Data
{
    public class lookupSvc : IService
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

        public PyDataType LookupCorporations(PyString criteria, PyDataType exactMatch, CallInformation call)
        {
            bool exact = false;

            if (exactMatch is PyBool exactBool)
                exact = exactBool;
            else if (exactMatch is PyInteger exactInteger)
                exact = exactInteger != 0;
            
            return this.DB.LookupCorporations(criteria, exact);
        }

        public PyDataType LookupCorporationTickers(PyString criteria, PyDataType exactMatch, CallInformation call)
        {
            bool exact = false;

            if (exactMatch is PyBool exactBool)
                exact = exactBool;
            else if (exactMatch is PyInteger exactInteger)
                exact = exactInteger != 0;
            
            return this.DB.LookupCorporationTickers(criteria, exact);
        }

        public PyDataType LookupFactions(PyString criteria, PyDataType exactMatch, CallInformation call)
        {
            bool exact = false;

            if (exactMatch is PyBool exactBool)
                exact = exactBool;
            else if (exactMatch is PyInteger exactInteger)
                exact = exactInteger != 0;
            
            return this.DB.LookupFactions(criteria, exact);
        }

        public PyDataType LookupKnownLocationsByGroup(PyInteger groupID, PyString searchStr, CallInformation call)
        {
            return this.DB.LookupKnownLocationsByGroup(searchStr, groupID);
        }
    }
}