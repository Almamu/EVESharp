using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Notifications.Skills;

public class OnGodmaMultipleSkillsTrained : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnGodmaMultipleSkillsTrained";

    public PyList <PyInteger> SkillTypeIDs { get; }

    public OnGodmaMultipleSkillsTrained (PyList <PyInteger> skillTypeIDs) : base (NOTIFICATION_NAME)
    {
        this.SkillTypeIDs = skillTypeIDs;
    }

    public override List <PyDataType> GetElements ()
    {
        return new List <PyDataType> {this.SkillTypeIDs};
    }
}