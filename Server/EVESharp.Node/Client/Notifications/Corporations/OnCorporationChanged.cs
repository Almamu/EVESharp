using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Client.Notifications.Corporations;

public class OnCorporationChanged : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnCorporationChanged";

    public int          CorporationID { get; init; }
    public PyDictionary Changes       { get; init; }

    public OnCorporationChanged (int corporationID) : base (NOTIFICATION_NAME)
    {
        CorporationID = corporationID;
        Changes       = new PyDictionary ();
    }

    public OnCorporationChanged AddChange (string changeName, PyDataType oldValue, PyDataType newValue)
    {
        Changes [changeName] = new PyTuple (2)
        {
            [0] = oldValue,
            [1] = newValue
        };

        return this;
    }

    public override List <PyDataType> GetElements ()
    {
        return new List <PyDataType>
        {
            CorporationID,
            Changes
        };
    }
}