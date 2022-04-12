using System;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Server.Shared.Transports;

public class MachoClientTransport : MachoTransport
{
    public MachoClientTransport (MachoTransport source) : base (source)
    {
        // finally assign the correct packet handler
        Socket.SetReceiveCallback (this.ReceiveNormalPacket);
        Socket.SetExceptionHandler (this.HandleException);
        Socket.SetOnConnectionLostHandler (this.HandleConnectionLost);
    }

    private void HandleConnectionLost ()
    {
        Log.Fatal ("Client {0} lost connection to the server", Session.UserID);

        // clean up ourselves
        MachoNet.OnTransportTerminated (this);
    }

    private void HandleException (Exception ex)
    {
        Log.Error ("Exception detected: ");

        do
        {
            Log.Error ("{0}\n{1}", ex.Message, ex.StackTrace);
        }
        while ((ex = ex.InnerException) != null);
    }

    private void ReceiveNormalPacket (PyDataType packet)
    {
        if (packet is PyObject)
            throw new Exception ("Got exception from client");

        PyPacket pyPacket = packet;

        // replace the address if specific situations occur (why is CCP doing it like this?)
        if (pyPacket.Type == PyPacket.PacketType.NOTIFICATION && pyPacket.Source is PyAddressNode)
            pyPacket.Source = new PyAddressClient (Session.UserID);

        // ensure the source address is right as it cannot be trusted
        if (pyPacket.Source is not PyAddressClient source)
            throw new Exception ("Received a packet from client without a source client address");
        if (pyPacket.UserID != Session.UserID)
            throw new Exception ("Received a packet coming from a client trying to spoof it's userID");

        // ensure the clientId is set in the PyAddressClient
        source.ClientID = Session.UserID;

        // queue the input packet into machoNet so it handles it
        MachoNet.QueueInputPacket (this, pyPacket);
    }
}