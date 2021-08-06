using System;
using EVE.Packets.Complex;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Nodes.Corps
{
    public class OnCorporationChanged : InterNodeNotification
    {
        public const string NOTIFICATION_NAME = "OnCorporationChanged";
        
        public int CorporationID { get; init; }
        public int? AllianceID { get; init; }
        public int? ExecutorCorpID { get; init; }
        
        public OnCorporationChanged(int corporationID, int? allianceID, int? executorCorpID) : base(NOTIFICATION_NAME)
        {
            this.CorporationID = corporationID;
            this.AllianceID = allianceID;
            this.ExecutorCorpID = executorCorpID;
        }

        protected override PyDataType GetNotification()
        {
            return new PyTuple(3)
            {
                [0] = this.CorporationID,
                [1] = this.AllianceID,
                [2] = this.ExecutorCorpID
            };
        }
        
        public static implicit operator OnCorporationChanged(PyTuple notification)
        {
            if (notification.Count != 2)
                throw new InvalidCastException("Expected a tuple with two items");
            if (notification[0] is not PyString name || name != NOTIFICATION_NAME)
                throw new InvalidCastException($"Expected a {NOTIFICATION_NAME}");
            if (notification[1] is not PyTuple data)
                throw new InvalidCastException("Expected a tuple as the first element");
            if (data.Count != 3)
                throw new InvalidCastException("Expected a tuple with three items");
            if (data[0] is not PyInteger corporationID)
                throw new InvalidCastException("Expected a corporationID");
            if (data[1] is not null && data[1] is not PyInteger)
                throw new InvalidCastException("Expected a allianceID");
            if (data[2] is not null && data[2] is not PyInteger)
                throw new InvalidCastException("Expected a executorCorpID");

            return new OnCorporationChanged(corporationID, data[1] as PyInteger, data[2] as PyInteger);
        }
    }
}