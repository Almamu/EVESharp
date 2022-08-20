using System;
using EVESharp.Common.Network.Sockets;
using EVESharp.EVE.Sessions;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;
using Serilog;

namespace EVESharp.EVE.Network.Transports;

public class MachoClientTransport : IMachoTransport
{
    public Session                        Session          { get; }
    public ILogger                        Log              { get; }
    public IEVESocket                     Socket           { get; }
    public IMachoNet                      MachoNet         { get; }
    public ITransportManager              TransportManager { get; }
    public event Action <IMachoTransport> OnTerminated;

    public MachoClientTransport (IMachoTransport source)
    {
        this.Socket           = source.Socket;
        this.Log              = source.Log;
        this.Session          = source.Session;
        this.MachoNet         = source.MachoNet;
        this.TransportManager = source.TransportManager;
        
        // finally assign the correct packet handler
        this.Socket.OnDataReceived   += this.ReceiveNormalPacket;
        this.Socket.OnException      += this.HandleException;
        this.Socket.OnConnectionLost += this.HandleConnectionLost;
    }

    private void HandleConnectionLost ()
    {
        this.Log.Error ("Client {0} lost connection to the server", this.Session.UserID);

        // clean up ourselves
        this.OnTerminated (this);
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

    public void Close ()
    {
        this.Dispose ();
    }
    
    public void Dispose ()
    {
        // finally close the socket
        this.Socket.Close ();
        
        // cleanup callbacks
        this.Socket.OnDataReceived   -= this.ReceiveNormalPacket;
        this.Socket.OnException      -= this.HandleException;
        this.Socket.OnConnectionLost -= this.HandleConnectionLost;
    }
}