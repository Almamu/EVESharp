using System;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Network.Transports;

public class MachoClientTransport : MachoTransport
{
    public MachoClientTransport (MachoTransport source) : base (source)
    {
        // finally assign the correct packet handler
        this.Socket.SetReceiveCallback (this.ReceiveNormalPacket);
        this.Socket.SetExceptionHandler (this.HandleException);
        this.Socket.SetOnConnectionLostHandler (this.HandleConnectionLost);
    }

    private void HandleConnectionLost ()
    {
        this.Log.Fatal ("Client {0} lost connection to the server", this.Session.UserID);

        // clean up ourselves
        this.MachoNet.OnTransportTerminated (this);
    }

    private void HandleException (Exception ex)
    {
        this.Log.Error ("Exception detected: ");

        do
        {
            this.Log.Error ("{0}\n{1}", ex.Message, ex.StackTrace);
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
            pyPacket.Source = new PyAddressClient (this.Session.UserID);

        // ensure the source address is right as it cannot be trusted
        if (pyPacket.Source is not PyAddressClient source)
            throw new Exception ("Received a packet from client without a source client address");
        if (pyPacket.UserID != this.Session.UserID)
            throw new Exception ("Received a packet coming from a client trying to spoof it's userID");

        // ensure the clientId is set in the PyAddressClient
        source.ClientID = this.Session.UserID;

        // queue the input packet into machoNet so it handles it
        this.MachoNet.QueueInputPacket (this, pyPacket);
    }
}