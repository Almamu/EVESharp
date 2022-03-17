using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Sessions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Notifications.Client.Station
{
    public class OnCharNowInStation : ClientNotification
    {
        private const string NOTIFICATION_NAME = "OnCharNowInStation";
        
        public int? CharacterID { get; init; }
        public int? CorporationID { get; init; }
        public int? AllianceID { get; init; }
        public int? WarFactionID { get; init; }
        
        public OnCharNowInStation(Session session) : base(NOTIFICATION_NAME)
        {
            this.CharacterID = session.CharacterID;
            this.CorporationID = session.CorporationID;
            this.AllianceID = session.AllianceID;
            this.WarFactionID = session.WarFactionID;
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