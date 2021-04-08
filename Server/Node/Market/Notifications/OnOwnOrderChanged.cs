using System.Collections.Generic;
using System.Security.Cryptography;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Market.Notifications
{
    public class OnOwnOrderChanged : PyMultiEventEntry
    {
        private static readonly DBRowDescriptor DESCRIPTOR = new DBRowDescriptor()
        {
            Columns =
            {
                new DBRowDescriptor.Column("typeID", FieldType.I2),
            }
        };
        
        private const string NOTIFICATION_NAME = "OnOwnOrderChanged";
        
        public PyString Reason { get; }
        public int TypeID { get; }
        public bool IsCorp { get; }
        
        public OnOwnOrderChanged(int typeID, string reason, bool isCorp = false) : base(NOTIFICATION_NAME)
        {
            this.TypeID = typeID;
            this.Reason = reason;
            this.IsCorp = isCorp;
        }

        public override List<PyDataType> GetElements()
        {
            return new List<PyDataType>()
            {
                new PyPackedRow(DESCRIPTOR, new Dictionary<string, PyDataType>() { {"typeID", this.TypeID} }),
                this.Reason,
                this.IsCorp
            };
        }
    }
}