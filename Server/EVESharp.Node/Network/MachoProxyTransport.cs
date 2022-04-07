using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Network;

public class MachoProxyTransport : MachoTransport
{
    public MachoProxyTransport (MachoTransport source) : base (source)
    {
        Socket.SetReceiveCallback (this.HandleProxyPacket);
        this.SendPostAuthenticationPackets ();
    }

    private void HandleProxyPacket (PyDataType data)
    {
        // these should directly be PyPackets
        PyPacket packet = data;

        MachoNet.QueueInputPacket (this, packet);
    }
}