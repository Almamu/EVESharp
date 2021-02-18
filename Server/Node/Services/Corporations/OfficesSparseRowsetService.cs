using System.Collections.Generic;
using Node.Database;
using Node.Inventory.Items.Types;
using Node.Network;
using Node.Services.Database;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Services.Corporations
{
    public class OfficesSparseRowsetService : SparseRowsetDatabaseService
    {
        private Dictionary<PyDataType, int> RowsIndex = new Dictionary<PyDataType, int>();
        private Corporation Corporation { get; }
        private CorporationDB DB { get; }
        
        public OfficesSparseRowsetService(Corporation corporation, CorporationDB db, SparseRowsetHeader rowsetHeader, BoundServiceManager manager, Client client) : base(rowsetHeader, manager, client)
        {
            this.DB = db;
            this.Corporation = corporation;
            
            // get all the indexes based on the key
            this.RowsIndex = this.DB.GetOffices(corporation.ID);
        }

        public override PyDataType Fetch(PyInteger startPos, PyInteger fetchSize, CallInformation call)
        {
            return this.DB.GetOffices(this.Corporation.ID, startPos, fetchSize, this.SparseRowset, this.RowsIndex);
        }

        public override PyDataType FetchByKey(PyList keyList, CallInformation call)
        {
            return this.DB.GetOffices(keyList, this.Corporation.ID, this.SparseRowset, this.RowsIndex);
        }
    }
}