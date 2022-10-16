using System;
using EVESharp.Database.Inventory;
using EVESharp.EVE.Exceptions.corpStationMgr;
using EVESharp.EVE.Packets.Complex;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Node.Notifications.Nodes.Corps;

public class OnOfficeFolderDestroyed : InterNodeNotification
{
    public const string NOTIFICATION_NAME = "OnOfficeFolderDestroyed";

    public PyInteger OfficeFolderID { get; }

    public OnOfficeFolderDestroyed (PyInteger officeFolderID) : base (NOTIFICATION_NAME)
    {
        this.OfficeFolderID = officeFolderID;
    }

    protected override PyDataType GetNotification ()
    {
        return this.OfficeFolderID;
    }

    public static implicit operator OnOfficeFolderDestroyed (PyTuple notification)
    {
        if (notification.Count != 2)
            throw new InvalidCastException ("Expected a tuple with one item");

        if (notification [0] is not PyString name || name != NOTIFICATION_NAME)
            throw new InvalidCastException ($"Expected a {NOTIFICATION_NAME}");

        if (notification [1] is not PyInteger officeFolderID)
            throw new InvalidCastException ("Expected an integer as the first element");

        return new OnOfficeFolderDestroyed (officeFolderID);
    }
}