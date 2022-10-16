using System;
using EVESharp.EVE.Packets.Complex;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Node.Notifications.Nodes.Corps;

public class OnCorporationOfficeUnrented : InterNodeNotification
{
    public const string NOTIFICATION_NAME = "OnCorporationOfficeUnrented";

    public int CorporationID  { get; init; }
    public int OfficeFolderID { get; init; }

    public OnCorporationOfficeUnrented () : base (NOTIFICATION_NAME) { }

    protected override PyDataType GetNotification ()
    {
        return new PyTuple (2)
        {
            [0] = CorporationID,
            [1] = OfficeFolderID
        };
    }

    public static implicit operator OnCorporationOfficeUnrented (PyTuple notification)
    {
        if (notification.Count != 2)
            throw new InvalidCastException ("Expected a tuple with two items");

        if (notification [0] is not PyString name || name != NOTIFICATION_NAME)
            throw new InvalidCastException ($"Expected a {NOTIFICATION_NAME}");

        if (notification [1] is not PyTuple data)
            throw new InvalidCastException ("Expected a tuple as the first element");

        if (data.Count != 2)
            throw new InvalidCastException ("Expected tuple with four items");
        if (data [0] is not PyInteger corporationID)
            throw new InvalidCastException ("Expected an integer as first element");
        if (data [1] is not PyInteger officeFolderID)
            throw new InvalidCastException ("expected integer as fourth element");

        return new OnCorporationOfficeUnrented
        {
            CorporationID  = corporationID,
            OfficeFolderID = officeFolderID
        };
    }
}