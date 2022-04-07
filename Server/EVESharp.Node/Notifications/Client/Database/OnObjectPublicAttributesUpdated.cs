using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.Node.Services.Database;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Notifications.Client.Database;

public class OnObjectPublicAttributesUpdated : ClientNotification
{
    public const string                           NOTIFICATION_NAME = "OnObjectPublicAttributesUpdated";
    public       SparseRowsetDatabaseService      SparseRowset       { get; init; }
    public       PyDictionary <PyString, PyTuple> Changes            { get; init; }
    public       PyDictionary                     NotificationParams { get; init; }
    public       PyDataType                       PrimaryKey         { get; init; }

    public OnObjectPublicAttributesUpdated (
        PyDataType primaryKey, SparseRowsetDatabaseService rowset, PyDictionary <PyString, PyTuple> changes, PyDictionary notificationParams = null
    ) : base (NOTIFICATION_NAME)
    {
        PrimaryKey         = primaryKey;
        SparseRowset       = rowset;
        Changes            = changes;
        NotificationParams = notificationParams;
    }

    public override List <PyDataType> GetElements ()
    {
        return new List <PyDataType>
        {
            SparseRowset.BoundString,
            new PyDictionary {["realRowCount"] = SparseRowset.RowsetHeader.Count},
            new PyTuple (0),
            new PyDictionary
            {
                ["change"]             = Changes,
                ["changePKIndexValue"] = PrimaryKey,
                ["partials"]           = new PyList {"realRowCount"},
                ["notificationParams"] = NotificationParams ?? new PyDictionary ()
            }
        };
    }
}