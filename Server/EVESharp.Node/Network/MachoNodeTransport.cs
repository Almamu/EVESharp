using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Network;

public class MachoNodeTransport : MachoTransport
{
    public MachoNodeTransport (MachoTransport source) : base (source)
    {
        // add load status to the session
        Session.LoadMetric = 0;
        Socket.SetReceiveCallback (this.HandlePacket);
        this.SendPostAuthenticationPackets ();
    }

    private void HandlePacket (PyDataType data)
    {
        PyPacket packet = data;

        MachoNet.QueueInputPacket (this, data);
    }
}