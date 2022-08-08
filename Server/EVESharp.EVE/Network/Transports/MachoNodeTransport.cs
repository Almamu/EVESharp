using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Network.Transports;

public class MachoNodeTransport : MachoTransport
{
    public MachoNodeTransport (MachoTransport source) : base (source)
    {
        // add load status to the session
        this.Session.LoadMetric = 0;
        this.Socket.SetReceiveCallback (this.HandlePacket);
        this.SendPostAuthenticationPackets ();
    }

    private void HandlePacket (PyDataType data)
    {
        PyPacket packet = data;

        this.MachoNet.QueueInputPacket (this, data);
    }
}