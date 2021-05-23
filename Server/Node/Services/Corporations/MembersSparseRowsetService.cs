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
    public class MembersSparseRowsetService : SparseRowsetDatabaseService
    {
        private Dictionary<PyDataType, int> RowsIndex = new Dictionary<PyDataType, int>();
        private Corporation Corporation { get; }
        private CorporationDB DB { get; }
        public MembersSparseRowsetService(Corporation corporation, CorporationDB db, SparseRowsetHeader rowsetHeader, BoundServiceManager manager, Client client) : base(rowsetHeader, manager, client)
        {
            this.DB = db;
            this.Corporation = corporation;
            
            // get all the indexes based on the key
            this.RowsIndex = this.DB.GetMembers(corporation.ID);
        }

        public override PyDataType Fetch(PyInteger startPos, PyInteger fetchSize, CallInformation call)
        {
            return this.DB.GetMembers(this.Corporation.ID, startPos, fetchSize, this.RowsetHeader, this.RowsIndex);
        }

        public override PyDataType FetchByKey(PyList keyList, CallInformation call)
        {
            return this.DB.GetMembers(keyList.GetEnumerable<PyInteger>(), this.Corporation.ID, this.RowsetHeader, this.RowsIndex);
        }

        public override PyDataType SelectByUniqueColumnValues(PyString columnName, PyList values, CallInformation call)
        {
            throw new System.NotImplementedException();
        }

        public override void SendOnObjectChanged(int primaryKey)
        {
            throw new System.NotImplementedException();
        }

        public override bool IsClientAllowedToCall(CallInformation call)
        {
            return call.Client.CorporationID == this.Corporation.ID;
        }
    }
}