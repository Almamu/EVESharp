using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Client.Notifications.Contracts;

public class OnContractAccepted : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnContractAccepted";

    public int ContractID { get; init; }

    public OnContractAccepted (int contractID) : base (NOTIFICATION_NAME)
    {
        ContractID = contractID;
    }

    public override List <PyDataType> GetElements ()
    {
        return new List <PyDataType>
        {
            ContractID,
            null
        };
    }
}