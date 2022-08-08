using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Network.Transports;

public class MachoProxyTransport : MachoTransport
{
    public MachoProxyTransport (MachoTransport source) : base (source)
    {
        this.Socket.SetReceiveCallback (this.HandleProxyPacket);
        this.SendPostAuthenticationPackets ();
    }

    private void HandleProxyPacket (PyDataType data)
    {
        // these should directly be PyPackets
        PyPacket packet = data;

        this.MachoNet.QueueInputPacket (this, packet);
    }
}