using System;
using EVE.Packets.Complex;
using Node.Exceptions.corpStationMgr;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Notifications.Nodes.Corporations
{
    public class OnCorporationMemberUpdated : InterNodeNotification
    {
        public const string NOTIFICATION_NAME = "OnCorporationMemberUpdated";

        public int CharacterID { get; init; }
        public long Roles { get; init; }
        public long GrantableRoles { get; init; }
        public long RolesAtHQ { get; init; }
        public long GrantableRolesAtHQ { get; init; }
        public long RolesAtBase { get; init; }
        public long GrantableRolesAtBase { get; init; }
        public long RolesAtOther { get; init; }
        public long GrantableRolesAtOther { get; init; }
        public int? BaseID { get; init; }
        public bool BlockRoles { get; init; }
        public long TitleMask { get; init; }

        public OnCorporationMemberUpdated(int characterID, long roles, long grantableRoles, long rolesAtHQ, long grantableRolesAtHQ,
            long rolesAtBase, long grantableRolesAtBase, long rolesAtOther, long grantableRolesAtOther, int? baseID,
            bool blockRoles, long titleMask) : base(NOTIFICATION_NAME)
        {
            this.CharacterID = characterID;
            this.Roles = roles;
            this.GrantableRoles = grantableRoles;
            this.RolesAtHQ = rolesAtHQ;
            this.GrantableRolesAtHQ = grantableRolesAtHQ;
            this.RolesAtBase = rolesAtBase;
            this.GrantableRolesAtBase = grantableRolesAtBase;
            this.RolesAtOther = rolesAtOther;
            this.GrantableRolesAtOther = grantableRolesAtOther;
            this.BaseID = baseID;
            this.BlockRoles = blockRoles;
            this.TitleMask = titleMask;
        }

        protected override PyDataType GetNotification()
        {
            return new PyDictionary
            {
                ["characterID"] = this.CharacterID,
                ["roles"] = this.Roles,
                ["grantableRoles"] = this.GrantableRoles,
                ["rolesAtHQ"] = this.RolesAtHQ,
                ["grantableRolesAtHQ"] = this.GrantableRolesAtHQ,
                ["rolesAtBase"] = this.RolesAtBase,
                ["grantableRolesAtBase"] = this.GrantableRolesAtBase,
                ["rolesAtOther"] = this.RolesAtOther,
                ["grantableRolesAtOther"] = this.GrantableRolesAtOther,
                ["baseID"] = this.BaseID,
                ["blockRoles"] = this.BlockRoles,
                ["titleMask"] = this.TitleMask
            };
        }
        

        public static implicit operator OnCorporationMemberUpdated(PyTuple notification)
        {
            if (notification.Count != 2)
                throw new InvalidCastException("Expected a tuple with two items");
            if (notification[0] is not PyString name || name != NOTIFICATION_NAME)
                throw new InvalidCastException($"Expected a {NOTIFICATION_NAME}");
            if (notification[1] is not PyDictionary data)
                throw new InvalidCastException("Expected a dictionary as the first element");
            
            data.TryGetValue("characterID", out PyInteger characterID);
            data.TryGetValue("roles", out PyInteger roles);
            data.TryGetValue("grantableRoles", out PyInteger grantableRoles);
            data.TryGetValue("rolesAtHQ", out PyInteger rolesAtHQ);
            data.TryGetValue("grantableRolesAtHQ", out PyInteger grantableRolesAtHQ);
            data.TryGetValue("rolesAtBase", out PyInteger rolesAtBase);
            data.TryGetValue("grantableRolesAtBase", out PyInteger grantableRolesAtBase);
            data.TryGetValue("rolesAtOther", out PyInteger rolesAtOther);
            data.TryGetValue("grantableRolesAtOther", out PyInteger grantableRolesAtOther);
            data.TryGetValue("baseID", out PyInteger baseID);
            data.TryGetValue("blockRoles", out PyBool blockRoles);
            data.TryGetValue("titleMask", out PyInteger titleMask);

            return new OnCorporationMemberUpdated(characterID, roles, grantableRoles, rolesAtHQ, grantableRolesAtHQ, rolesAtBase, grantableRolesAtBase, rolesAtOther, grantableRolesAtOther, baseID, blockRoles, titleMask);
        }
    }
}