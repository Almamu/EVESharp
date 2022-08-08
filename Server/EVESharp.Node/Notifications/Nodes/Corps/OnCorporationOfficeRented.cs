using System;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Notifications.Nodes.Corps;

public class OnCorporationOfficeRented : InterNodeNotification
{
    public const string NOTIFICATION_NAME = "OnCorporationOfficeRented";

    public int CorporationID  { get; init; }
    public int StationID      { get; init; }
    public int TypeID         { get; init; }
    public int OfficeFolderID { get; init; }

    public OnCorporationOfficeRented () : base (NOTIFICATION_NAME) { }

    protected override PyDataType GetNotification ()
    {
        return new PyTuple (4)
        {
            [0] = CorporationID,
            [1] = StationID,
            [2] = TypeID,
            [3] = OfficeFolderID
        };
    }

    public static implicit operator OnCorporationOfficeRented (PyTuple notification)
    {
        if (notification.Count != 2)
            throw new InvalidCastException ("Expected a tuple with two items");

        if (notification [0] is not PyString name || name != NOTIFICATION_NAME)
            throw new InvalidCastException ($"Expected a {NOTIFICATION_NAME}");

        if (notification [1] is not PyTuple data)
            throw new InvalidCastException ("Expected a tuple as the first element");

        if (data.Count != 4)
            throw new InvalidCastException ("Expected tuple with four items");

        if (data [0] is not PyInteger corporationID)
            throw new InvalidCastException ("Expected an integer as first element");

        if (data [1] is not PyInteger stationID)
            throw new InvalidCastException ("Expected integer as second element");

        if (data [2] is not PyInteger typeID)
            throw new InvalidCastException ("Expected integer as third element");

        if (data [3] is not PyInteger officeFolderID)
            throw new InvalidCastException ("expected integer as fourth element");

        return new OnCorporationOfficeRented
        {
            CorporationID  = corporationID,
            StationID      = stationID,
            TypeID         = typeID,
            OfficeFolderID = officeFolderID
        };
    }
}