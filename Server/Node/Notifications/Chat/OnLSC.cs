using System.Collections.Generic;
using Node.Network;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Chat
{
    public class OnLSC : PyNotification
    {
        private const string NOTIFICATION_NAME = "OnLSC";
        
        public int? AllianceID { get; init; }
        public int? CorporationID { get; init; }
        public int? CharacterID { get; init; }
        public long? Role { get; init; }
        public long? CorporationRole { get; init; }
        public int? WarFactionID { get; init; }
        public string Type { get; init; }
        public PyDataType Channel { get; init; }
        public PyTuple Arguments { get; init; }
        
        public OnLSC(Client client, string type, PyDataType channel, PyTuple args) : base(NOTIFICATION_NAME)
        {
            this.AllianceID = client.AllianceID;
            this.CorporationID = client.CorporationID;
            this.CharacterID = client.CharacterID;
            this.Role = client.Role;
            this.CorporationRole = client.CorporationRole;
            this.WarFactionID = client.WarFactionID;
            this.Type = type;
            this.Channel = channel;
            this.Arguments = args;
        }

        public override List<PyDataType> GetElements()
        {
            PyTuple who = new PyTuple(6)
            {
                [0] = this.AllianceID,
                [1] = this.CorporationID,
                [2] = this.CharacterID,
                [3] = this.Role,
                [4] = this.CorporationRole,
                [5] = this.WarFactionID
            };

            return new List<PyDataType>()
            {
                this.Channel,
                1,
                this.Type,
                who,
                this.Arguments
            };
        }
    }
}