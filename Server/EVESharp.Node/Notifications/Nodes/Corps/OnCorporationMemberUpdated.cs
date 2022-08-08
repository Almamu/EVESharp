using System;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Notifications.Nodes.Corps;

public class OnCorporationMemberUpdated : InterNodeNotification
{
    public const string NOTIFICATION_NAME = "OnCorporationMemberUpdated";

    public int  CharacterID           { get; init; }
    public long Roles                 { get; init; }
    public long GrantableRoles        { get; init; }
    public long RolesAtHQ             { get; init; }
    public long GrantableRolesAtHQ    { get; init; }
    public long RolesAtBase           { get; init; }
    public long GrantableRolesAtBase  { get; init; }
    public long RolesAtOther          { get; init; }
    public long GrantableRolesAtOther { get; init; }
    public int? BaseID                { get; init; }
    public int? BlockRoles            { get; init; }
    public long TitleMask             { get; init; }

    public OnCorporationMemberUpdated (
        int  characterID, long roles,                long grantableRoles, long rolesAtHQ,             long grantableRolesAtHQ,
        long rolesAtBase, long grantableRolesAtBase, long rolesAtOther,   long grantableRolesAtOther, int? baseID,
        int? blockRoles,  long titleMask
    ) : base (NOTIFICATION_NAME)
    {
        CharacterID           = characterID;
        Roles                 = roles;
        GrantableRoles        = grantableRoles;
        RolesAtHQ             = rolesAtHQ;
        GrantableRolesAtHQ    = grantableRolesAtHQ;
        RolesAtBase           = rolesAtBase;
        GrantableRolesAtBase  = grantableRolesAtBase;
        RolesAtOther          = rolesAtOther;
        GrantableRolesAtOther = grantableRolesAtOther;
        BaseID                = baseID;
        BlockRoles            = blockRoles;
        TitleMask             = titleMask;
    }

    protected override PyDataType GetNotification ()
    {
        return new PyDictionary
        {
            ["characterID"]           = CharacterID,
            ["roles"]                 = Roles,
            ["grantableRoles"]        = GrantableRoles,
            ["rolesAtHQ"]             = RolesAtHQ,
            ["grantableRolesAtHQ"]    = GrantableRolesAtHQ,
            ["rolesAtBase"]           = RolesAtBase,
            ["grantableRolesAtBase"]  = GrantableRolesAtBase,
            ["rolesAtOther"]          = RolesAtOther,
            ["grantableRolesAtOther"] = GrantableRolesAtOther,
            ["baseID"]                = BaseID,
            ["blockRoles"]            = BlockRoles,
            ["titleMask"]             = TitleMask
        };
    }
    
    public static implicit operator OnCorporationMemberUpdated (PyTuple notification)
    {
        if (notification.Count != 2)
            throw new InvalidCastException ("Expected a tuple with two items");
        if (notification [0] is not PyString name || name != NOTIFICATION_NAME)
            throw new InvalidCastException ($"Expected a {NOTIFICATION_NAME}");
        if (notification [1] is not PyDictionary data)
            throw new InvalidCastException ("Expected a dictionary as the first element");

        data.TryGetValue ("characterID",           out PyInteger characterID);
        data.TryGetValue ("roles",                 out PyInteger roles);
        data.TryGetValue ("grantableRoles",        out PyInteger grantableRoles);
        data.TryGetValue ("rolesAtHQ",             out PyInteger rolesAtHQ);
        data.TryGetValue ("grantableRolesAtHQ",    out PyInteger grantableRolesAtHQ);
        data.TryGetValue ("rolesAtBase",           out PyInteger rolesAtBase);
        data.TryGetValue ("grantableRolesAtBase",  out PyInteger grantableRolesAtBase);
        data.TryGetValue ("rolesAtOther",          out PyInteger rolesAtOther);
        data.TryGetValue ("grantableRolesAtOther", out PyInteger grantableRolesAtOther);
        data.TryGetValue ("baseID",                out PyInteger baseID);
        data.TryGetValue ("blockRoles",            out PyInteger blockRoles);
        data.TryGetValue ("titleMask",             out PyInteger titleMask);

        return new OnCorporationMemberUpdated (
            characterID, roles, grantableRoles, rolesAtHQ, grantableRolesAtHQ, rolesAtBase, grantableRolesAtBase, rolesAtOther,
            grantableRolesAtOther, baseID, blockRoles, titleMask
        );
    }
}