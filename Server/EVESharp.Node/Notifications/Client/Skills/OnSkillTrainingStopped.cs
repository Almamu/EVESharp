using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Notifications.Client.Skills;

public class OnSkillTrainingStopped : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnSkillTrainingStopped";

    /// <summary>
    /// The skill this notification is about
    /// </summary>
    public Skill Skill { get; }

    public OnSkillTrainingStopped (Skill skill) : base (NOTIFICATION_NAME)
    {
        Skill = skill;
    }

    public override List <PyDataType> GetElements ()
    {
        return new List <PyDataType>
        {
            Skill.ID,
            0
        };
    }
}