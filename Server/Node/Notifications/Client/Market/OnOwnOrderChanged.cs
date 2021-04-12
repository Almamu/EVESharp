using System.Collections.Generic;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Client.Market
{
    public class OnOwnOrderChanged : PyNotification
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