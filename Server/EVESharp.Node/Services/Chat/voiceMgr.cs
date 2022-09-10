using EVESharp.EVE.Services;
using EVESharp.Types;

namespace EVESharp.Node.Services.Chat;

public class voiceMgr : Service
{
    public override AccessLevel AccessLevel => AccessLevel.Location;

    public PyDataType VoiceEnabled (CallInformation call)
    {
        return 0;
    }
}