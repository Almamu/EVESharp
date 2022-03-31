using EVESharp.EVE.Sessions;
using EVESharp.PythonTypes;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Network;

public class MachoNodeTransport : MachoTransport
{
    public MachoNodeTransport(MachoTransport source) : base(source)
    {
        // add load status to the session
        this.Session.LoadMetric = 0;
        this.Socket.SetReceiveCallback(HandlePacket);
        this.SendPostAuthenticationPackets();
    }

    private void HandlePacket(PyDataType data)
    {
        PyPacket packet = data;
        
        this.MachoNet.QueueInputPacket(this, data);
    }
}