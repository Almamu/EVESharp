using System.Collections.Generic;
using EVE.Packets.Complex;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Client.Corporations
{
    public class OnCorporationVoteChanged : ClientNotification
    {
        private const string NOTIFICATION_NAME = "OnCorporationVoteChanged";
        
        public int CorporationID { get; init; }
        public int VoteCaseID { get; init; }
        public int CharacterID { get; init; }
        private PyDictionary<PyString,PyTuple> Changes { get; init; }
        
        public OnCorporationVoteChanged(int corporationID, int voteCaseID, int characterID) : base(NOTIFICATION_NAME)
        {
            this.CorporationID = corporationID;
            this.VoteCaseID = voteCaseID;
            this.CharacterID = characterID;
            this.Changes = new PyDictionary<PyString, PyTuple>();
        }

        public OnCorporationVoteChanged AddValue(string columnName, PyDataType oldValue, PyDataType newValue)
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
                this.CharacterID,
                this.Changes
            };
        }
    }
}