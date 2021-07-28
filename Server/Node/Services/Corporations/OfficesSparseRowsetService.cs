using System.Collections.Generic;
using Node.Database;
using Node.Inventory.Items.Types;
using Node.Network;
using Node.Services.Database;
using PythonTypes.Types.Collections;
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
            return this.DB.GetOffices(this.Corporation.ID, startPos, fetchSize, this.RowsetHeader, this.RowsIndex);
        }

        public override PyDataType FetchByKey(PyList keyList, CallInformation call)
        {
            return this.DB.GetOffices(keyList.GetEnumerable<PyInteger>(), this.Corporation.ID, this.RowsetHeader, this.RowsIndex);
        }

        public override PyDataType SelectByUniqueColumnValues(PyString columnName, PyList values, CallInformation call)
        {
            return this.DB.GetOffices(columnName, values.GetEnumerable<PyInteger>(), this.Corporation.ID, this.RowsetHeader, this.RowsIndex);
        }

        protected override void SendOnObjectChanged(PyDataType primaryKey, PyDictionary<PyString, PyTuple> changes, PyDictionary notificationParams = null)
        {
            throw new System.NotImplementedException();
        }

        public override void AddRow(PyDataType primaryKey, PyDictionary<PyString, PyTuple> changes)
        {
            throw new System.NotImplementedException();
        }

        public override void UpdateRow(PyDataType primaryKey, PyDictionary<PyString, PyTuple> changes)
        {
            throw new System.NotImplementedException();
        }

        public override void RemoveRow(PyDataType primaryKey)
        {
            throw new System.NotImplementedException();
        }

        public override bool IsClientAllowedToCall(CallInformation call)
        {
            return call.Client.CorporationID == this.Corporation.ID;
        }
    }
}