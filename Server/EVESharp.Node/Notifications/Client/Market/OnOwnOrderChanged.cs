using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Notifications.Client.Market;

public class OnOwnOrderChanged : ClientNotification
{
    private const           string          NOTIFICATION_NAME = "OnOwnOrderChanged";
    private static readonly DBRowDescriptor DESCRIPTOR        = new DBRowDescriptor {Columns = {new DBRowDescriptor.Column ("typeID", FieldType.I2)}};

    public PyString Reason { get; }
    public int      TypeID { get; }
    public bool     IsCorp { get; }

    public OnOwnOrderChanged (int typeID, string reason, bool isCorp = false) : base (NOTIFICATION_NAME)
    {
        TypeID = typeID;
        Reason = reason;
        IsCorp = isCorp;
    }

    public override List <PyDataType> GetElements ()
    {
        return new List <PyDataType>
        {
            new PyPackedRow (DESCRIPTOR, new Dictionary <string, PyDataType> {{"typeID", TypeID}}),
            Reason,
            IsCorp
        };
    }
}