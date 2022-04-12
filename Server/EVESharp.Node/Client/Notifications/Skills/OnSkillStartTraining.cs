using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Client.Notifications.Skills;

public class OnSkillStartTraining : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnSkillStartTraining";

    /// <summary>
    /// The skill this notification is about
    /// </summary>
    public Skill Skill { get; }

    public OnSkillStartTraining (Skill skill) : base (NOTIFICATION_NAME)
    {
        Skill = skill;
    }

    public override List <PyDataType> GetElements ()
    {
        return new List <PyDataType>
        {
            Skill.ID,
            Skill.ExpiryTime
        };
    }
}