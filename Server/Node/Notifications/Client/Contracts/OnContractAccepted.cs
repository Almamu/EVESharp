﻿using System.Collections.Generic;
using EVE.Packets.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Client.Contracts
{
    public class OnContractAccepted : ClientNotification
    {
        private const string NOTIFICATION_NAME = "OnContractAccepted";
        
        public int ContractID { get; init; }
        
        public OnContractAccepted(int contractID) : base(NOTIFICATION_NAME)
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
}