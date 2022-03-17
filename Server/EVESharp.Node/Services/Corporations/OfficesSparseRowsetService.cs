using System.Collections.Generic;
using EVESharp.EVE.Services;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Network;
using EVESharp.Node.Services.Database;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Corporations
{
    public class OfficesSparseRowsetService : SparseRowsetDatabaseService
    {
        public override AccessLevel AccessLevel => AccessLevel.None;
        
        private Dictionary<PyDataType, int> RowsIndex = new Dictionary<PyDataType, int>();
        private Corporation Corporation { get; }
        private CorporationDB DB { get; }
        
        public OfficesSparseRowsetService(Corporation corporation, CorporationDB db, SparseRowsetHeader rowsetHeader, BoundServiceManager manager, Session session) : base(rowsetHeader, manager, session, true)
        {
            this.DB = db;
            this.Corporation = corporation;
            
            // get all the indexes based on the key
            this.RowsIndex = this.DB.GetOffices(corporation.ID);
        }

        public override PyDataType Fetch(PyInteger startPos, PyInteger fetchSize, CallInformation call)
        {
            return this.DB.GetOffices(this.Corporation.ID, startPos, fetchSize, this.RowsetHeader);
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

        public override bool IsClientAllowedToCall(ServiceCall call)
        {
            return call.Session.CorporationID == this.Corporation.ID;
        }
    }
}