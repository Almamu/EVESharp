using System.Collections.Generic;
using EVE.Packets.Complex;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Client.Alliances
{
    public class OnAllianceMemberChanged : ClientNotification
    {
        private const string NOTIFICATION_NAME = "OnAllianceMemberChanged";
        
        public int AllianceID { get; init; }
        public int CorpID { get; init; }
        public PyDictionary Changes { get; init; }
        
        public OnAllianceMemberChanged(int allianceID, int corpID) : base(NOTIFICATION_NAME)
        {
            this.AllianceID = allianceID;
            this.CorpID = corpID;
            this.Changes = new PyDictionary();
        }

        public OnAllianceMemberChanged AddChange(string changeName, PyDataType oldValue, PyDataType newValue)
        {
            this.Changes[changeName] = new PyTuple(2)
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
                this.AllianceID,
                this.CorpID,
                this.Changes
            };
        }
    }
}