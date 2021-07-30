using System.Collections.Generic;
using EVE.Packets.Complex;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Client.Corporations
{
    public class OnCorporationVoteCaseChanged : ClientNotification
    {
        private const string NOTIFICATION_NAME = "OnCorporationVoteCaseChanged";
        
        public int CorporationID { get; init; }
        public int VoteCaseID { get; init; }
        private PyDictionary<PyString,PyTuple> Changes { get; init; }
        
        public OnCorporationVoteCaseChanged(int corporationID, int voteCaseID) : base(NOTIFICATION_NAME)
        {
            this.CorporationID = corporationID;
            this.VoteCaseID = voteCaseID;
            this.Changes = new PyDictionary<PyString, PyTuple>();
        }

        public OnCorporationVoteCaseChanged AddValue(string columnName, PyDataType oldValue, PyDataType newValue)
        {
            this.Changes[columnName] = new PyTuple(2)
            {
                [0] = oldValue,
                [1] = newValue
            };

            return this;
        }

        public override List<PyDataType> GetElements()
        {
            return new List<PyDataType>()
            {
                this.CorporationID,
                this.VoteCaseID,
                this.Changes
            };
        }
    }
}