using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Client.Notifications.Corporations;

public class OnCorporationMemberChanged : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnCorporationMemberChanged";

    public int MemberID         { get; init; }
    public int OldCorporationID { get; init; }
    public int NewCorporationID { get; init; }

    public OnCorporationMemberChanged (int memberID, int oldCorporationID, int newCorporationID) : base (NOTIFICATION_NAME)
    {
        MemberID         = memberID;
        OldCorporationID = oldCorporationID;
        NewCorporationID = newCorporationID;
    }

    public override List <PyDataType> GetElements ()
    {
        return new List <PyDataType>
        {
            MemberID,
            new PyDictionary
            {
                ["corporationID"] = new PyTuple (2)
                {
                    [0] = OldCorporationID,
                    [1] = NewCorporationID
                }
            }
        };
    }
}