using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Client.Notifications.Wallet;

public class OnShareChange : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnShareChange";

    public int  ShareholderID { get; init; }
    public int  CorporationID { get; init; }
    public int? OldShares     { get; init; }
    public int? NewShares     { get; init; }

    public OnShareChange (int shareholderID, int corporationID, int? oldShares, int? newShares) : base (NOTIFICATION_NAME)
    {
        OldShares = oldShares;
        NewShares = newShares;

        if (oldShares == 0)
            OldShares = null;
        if (newShares == 0)
            NewShares = null;

        CorporationID = corporationID;
        ShareholderID = shareholderID;
    }

    public override List <PyDataType> GetElements ()
    {
        return new List <PyDataType>
        {
            ShareholderID,
            CorporationID,
            new PyDictionary
            {
                ["ownerID"] = new PyTuple (2)
                {
                    [0] = ShareholderID,
                    [1] = ShareholderID
                },
                ["corporationID"] = new PyTuple (2)
                {
                    [0] = CorporationID,
                    [1] = CorporationID
                },
                ["shares"] = new PyTuple (2)
                {
                    [0] = OldShares,
                    [1] = NewShares
                }
            }
        };
    }
}