using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Notifications.Client.Alliances
{
    public class OnAllianceRelationshipChanged : ClientNotification
    {
        private const string NOTIFICATION_NAME = "OnAllianceRelationshipChanged";
        
        public int AllianceID { get; init; }
        public int ToID { get; init; }
        public PyDictionary Changes { get; init; }
        
        public OnAllianceRelationshipChanged(int allianceID, int toID) : base(NOTIFICATION_NAME)
        {
            this.AllianceID = allianceID;
            this.ToID = toID;
            this.Changes = new PyDictionary();
        }

        public OnAllianceRelationshipChanged AddChange(string changeName, PyDataType oldValue, PyDataType newValue)
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
                this.ToID,
                this.Changes
            };
        }
    }
}