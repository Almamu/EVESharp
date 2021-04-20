using System.Collections.Generic;
using EVE.Packets.Complex;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Client.Station
{
    public class OnCharNowInStation : ClientNotification
    {
        private const string NOTIFICATION_NAME = "OnCharNowInStation";
        
        public int? CharacterID { get; init; }
        public int? CorporationID { get; init; }
        public int? AllianceID { get; init; }
        public int? WarFactionID { get; init; }
        
        public OnCharNowInStation(Network.Client client) : base(NOTIFICATION_NAME)
        {
            this.CharacterID = client.CharacterID;
            this.CorporationID = client.CorporationID;
            this.AllianceID = client.AllianceID;
            this.WarFactionID = client.WarFactionID;
        }

        public override List<PyDataType> GetElements()
        {
            return new List<PyDataType>()
            {
                new PyTuple(4)
                {
                    [0] = this.CharacterID,
                    [1] = this.CorporationID,
                    [2] = this.AllianceID,
                    [3] = this.WarFactionID
                }
            };
        }
    }
}