using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Notifications.Client.Corporations;

public class OnCorporationApplicationChanged : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnCorporationApplicationChanged";

    public  int                              CorporationID { get; init; }
    public  int                              ApplicantID   { get; init; }
    private PyDictionary <PyString, PyTuple> Changes       { get; }

    public OnCorporationApplicationChanged (int corporationID, int applicantID) : base (NOTIFICATION_NAME)
    {
        CorporationID = corporationID;
        ApplicantID   = applicantID;
        Changes       = new PyDictionary <PyString, PyTuple> ();
    }

    public OnCorporationApplicationChanged AddValue (string columnName, PyDataType oldValue, PyDataType newValue)
    {
        Changes [columnName] = new PyTuple (2)
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
            ApplicantID,
            CorporationID,
            Changes
        };
    }
}