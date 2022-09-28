using EVESharp.EVE.Network.Services;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Node.Services.Network;

public class clientStatsMgr : Service
{
    public override AccessLevel AccessLevel => AccessLevel.None;

    public PyDataType SubmitStats (ServiceCall call, PyTuple stats)
    {
        // this data is useless as we don't develop the game
        // it seems to contain memory usage, OS information, ping, etc
        return null;
    }
}