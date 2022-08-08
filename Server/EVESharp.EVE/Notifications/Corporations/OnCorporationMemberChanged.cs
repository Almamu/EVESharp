using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Notifications.Corporations;

public class OnCorporationMemberChanged : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnCorporationMemberChanged";

    public int MemberID         { get; init; }
    public int OldCorporationID { get; init; }
    public int NewCorporationID { get; init; }

    public OnCorporationMemberChanged (int memberID, int oldCorporationID, int newCorporationID) : base (NOTIFICATION_NAME)
    {
        this.MemberID         = memberID;
        this.OldCorporationID = oldCorporationID;
        this.NewCorporationID = newCorporationID;
    }

    public override List <PyDataType> GetElements ()
    {
        return new List <PyDataType>
        {
            this.MemberID,
            new PyDictionary
            {
                ["corporationID"] = new PyTuple (2)
                {
                    [0] = this.OldCorporationID,
                    [1] = this.NewCorporationID
                }
            }
        };
    }
}