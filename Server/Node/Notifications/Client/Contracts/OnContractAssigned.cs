﻿using System.Collections.Generic;
using EVE.Packets.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Client.Contracts
{
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
}