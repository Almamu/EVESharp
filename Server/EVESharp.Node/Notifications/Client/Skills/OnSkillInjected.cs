using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Notifications.Client.Skills;

public class OnSkillInjected : ClientNotification
{
    private const string NOTIFICATION_NAME = "OnSkillInjected";
        
    public OnSkillInjected() : base(NOTIFICATION_NAME)
    {
    }

    public override List<PyDataType> GetElements()
    {
        return null;
    }
}