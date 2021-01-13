using Common.Database;
using Common.Services;
using Node.Database;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.Config
{
    public class config : Service
    {
        private ConfigDB DB { get; }
        
        public config(ConfigDB db)
        {
            this.DB = db;
        }

        public PyDataType GetMultiOwnersEx(PyList ids, PyDictionary namedPayload, Client client)
        {
            // return item data from the entity table and call it a day
            return this.DB.GetMultiOwnersEx(ids);
        }

        public PyDataType GetMultiGraphicsEx(PyList ids, PyDictionary namedPayload, Client client)
        {
            return this.DB.GetMultiGraphicsEx(ids);
        }

        public PyDataType GetMultiLocationsEx(PyList ids, PyDictionary namedPayload, Client client)
        {
            return this.DB.GetMultiLocationsEx(ids);
        }

        public PyDataType GetMultiAllianceShortNamesEx(PyList ids, PyDictionary namedPayload, Client client)
        {
            return this.DB.GetMultiAllianceShortNamesEx(ids);
        }
    }
}