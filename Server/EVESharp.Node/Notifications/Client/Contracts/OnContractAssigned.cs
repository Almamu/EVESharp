using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Notifications.Client.Contracts;

public class OnContractAssigned : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnContractAssigned";
        
    public int ContractID { get; init; }
        
    public OnContractAssigned(int contractID) : base(NOTIFICATION_NAME)
    {
        this.ContractID = contractID;
    }

    public override List<PyDataType> GetElements()
    {
        return new List<PyDataType>()
        {
            this.ContractID,
            null
        };
    }
}