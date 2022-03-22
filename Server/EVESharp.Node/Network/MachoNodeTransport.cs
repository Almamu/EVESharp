using EVESharp.Common.Logging;
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

        if (packet.Destination is PyAddressAny)
        {
            // handle the packet locally as this one should not reach anyone in specific
            switch (packet.Type)
            {
                case PyPacket.PacketType.NOTIFICATION:
                    this.HandleNotification(packet);
                    break;
            }
        }
        else
        {
            this.Server.MachoNet.QueuePacket(data);
        }
    }

    private void HandleNotification(PyPacket packet)
    {
        // ensure the notification packet is valid
        // this packet is an internal one
        if (packet.Payload.Count != 2)
        {
            Log.Error("Received ClusterController notification with the wrong format");
            return;
        }

        if (packet.Payload[0] is not PyString notification)
        {
            Log.Error("Received ClusterController notification with the wrong format");
            return;
        }
            
        Log.Debug($"Received a notification from ClusterController of type {notification.Value}");
            
        switch (notification)
        {
            case "UpdateSessionAttributes":
                this.HandleUpdateSessionAttributes(packet.Payload[1] as PyTuple);
                break;
            default:
                Log.Fatal("Received notification with the wrong format");
                break;
        }
    }

    private void HandleUpdateSessionAttributes(PyTuple payload)
    {
        // very simple version for now, should properly handle these sometime in the future
        PyString idType = payload[0] as PyString;
        PyInteger id = payload[1] as PyInteger;
        PyDictionary newValues = payload[2] as PyDictionary;

        this.Server.MachoNet.SessionManager.PerformSessionUpdate(idType, id, Session.FromPyDictionary (newValues));
    }
}