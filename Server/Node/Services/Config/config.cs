using Common.Database;
using Node.Database;
using PythonTypes.Types.Primitives;

namespace Node.Services.Config
{
    public class config : Service
    {
        private ConfigDB mDB = null;
        public config(DatabaseConnection db, ServiceManager manager) : base(manager)
        {
            this.mDB = new ConfigDB(db);
        }

        public PyDataType GetMultiOwnersEx(PyList ids, PyDictionary namedPayload, Client client)
        {
            // return item data from the entity table and call it a day
            return this.mDB.GetMultiOwnersEx(ids);
        }
    }
}