using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Notifications.Contracts;

public class OnContractOutbid : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnContractOutbid";

    public int ContractID { get; init; }

    public OnContractOutbid (int contractID) : base (NOTIFICATION_NAME)
    {
        this.ContractID = contractID;
    }

    public override List <PyDataType> GetElements ()
    {
        return new List <PyDataType>
        {
            this.ContractID,
            null
        };
    }
}