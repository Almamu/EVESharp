using System.Collections.Generic;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Packets.Complex;
using EVESharp.Types;

namespace EVESharp.EVE.Notifications.Skills;

public class OnSkillTrained : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnSkillTrained";

    /// <summary>
    /// The skill this notification is about
    /// </summary>
    public Skill Skill { get; }

    public OnSkillTrained (Skill skill) : base (NOTIFICATION_NAME)
    {
        this.Skill = skill;
    }

    public override List <PyDataType> GetElements ()
    {
        return new List <PyDataType> {this.Skill.ID};
    }
}