using EVESharp.EVE.Network.Services;
using EVESharp.Types;

namespace EVESharp.Node.Services.Chat;

public class voiceMgr : Service
{
    public override AccessLevel AccessLevel => AccessLevel.Location;

    public PyDataType VoiceEnabled (ServiceCall call)
    {
        return 0;
    }
}