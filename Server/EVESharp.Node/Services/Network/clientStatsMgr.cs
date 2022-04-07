using EVESharp.EVE.Services;
using EVESharp.Node.Network;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Network;

public class clientStatsMgr : Service
{
    public override AccessLevel AccessLevel => AccessLevel.None;

    public PyDataType SubmitStats (PyTuple stats, CallInformation call)
    {
        // this data is useless as we don't develop the game
        // it seems to contain memory usage, OS information, ping, etc
        return null;
    }
}