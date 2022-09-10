using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Notifications.Wallet;

public class OnShareChange : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnShareChange";

    public int  ShareholderID { get; init; }
    public int  CorporationID { get; init; }
    public int? OldShares     { get; init; }
    public int? NewShares     { get; init; }

    public OnShareChange (int shareholderID, int corporationID, int? oldShares, int? newShares) : base (NOTIFICATION_NAME)
    {
        this.OldShares = oldShares;
        this.NewShares = newShares;

        if (oldShares == 0)
            this.OldShares = null;
        if (newShares == 0)
            this.NewShares = null;

        this.CorporationID = corporationID;
        this.ShareholderID = shareholderID;
    }

    public override List <PyDataType> GetElements ()
    {
        return new List <PyDataType>
        {
            this.ShareholderID,
            this.CorporationID,
            new PyDictionary
            {
                ["ownerID"] = new PyTuple (2)
                {
                    [0] = this.ShareholderID,
                    [1] = this.ShareholderID
                },
                ["corporationID"] = new PyTuple (2)
                {
                    [0] = this.CorporationID,
                    [1] = this.CorporationID
                },
                ["shares"] = new PyTuple (2)
                {
                    [0] = this.OldShares,
                    [1] = this.NewShares
                }
            }
        };
    }
}