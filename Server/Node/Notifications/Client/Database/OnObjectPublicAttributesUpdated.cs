using System.Collections.Generic;
using EVE.Packets.Complex;
using Node.Services.Database;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Client.Database
{
    public class OnObjectPublicAttributesUpdated : ClientNotification
    {
        public const string NOTIFICATION_NAME = "OnObjectPublicAttributesUpdated";
        public SparseRowsetDatabaseService SparseRowset { get; init; }
        public PyDictionary<PyString, PyTuple> Changes { get; init; }
        public PyDictionary NotificationParams { get; init; }
        public PyDataType PrimaryKey { get; init; }

        public OnObjectPublicAttributesUpdated(PyDataType primaryKey, SparseRowsetDatabaseService rowset, PyDictionary<PyString, PyTuple> changes, PyDictionary notificationParams = null) : base(NOTIFICATION_NAME)
        {
            this.PrimaryKey = primaryKey;
            this.SparseRowset = rowset;
            this.Changes = changes;
            this.NotificationParams = notificationParams;
        }

        public override List<PyDataType> GetElements()
        {
            return new List<PyDataType>()
            {
                this.SparseRowset.BoundString,
                new PyDictionary {["realRowCount"] = this.SparseRowset.RowsetHeader.Count},
                new PyTuple(0),
                new PyDictionary()
                {
                    ["change"] = this.Changes,
                    ["changePKIndexValue"] = this.PrimaryKey,
                    ["partials"] = new PyList() {"realRowCount"},
                    ["notificationParams"] = this.NotificationParams ?? new PyDictionary()
                }
            };
        }
    }
}