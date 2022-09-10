using System;
using EVESharp.EVE.Network.Sockets;
using EVESharp.EVE.Sessions;
using EVESharp.Types;
using Serilog;

namespace EVESharp.EVE.Network.Transports;

public class MachoNodeTransport : IMachoTransport
{
    public Session                        Session          { get; }
    public ILogger                        Log              { get; }
    public IEVESocket                     Socket           { get; }
    public IMachoNet                      MachoNet         { get; }
    public ITransportManager              TransportManager { get; }
    public event Action <IMachoTransport> Terminated;
    
    public MachoNodeTransport (IMachoTransport source)
    {
        this.Socket           = source.Socket;
        this.Log              = source.Log;
        this.Session          = source.Session;
        this.MachoNet         = source.MachoNet;
        this.TransportManager = source.TransportManager;
        // add load status to the session
        this.Session.LoadMetric      =  0;
        this.Socket.DataReceived   += this.HandlePacket;
        this.Socket.Exception      += this.HandleException;
        this.Socket.ConnectionLost += this.HandleConnectionLost;
    }

    private void HandlePacket (PyDataType data)
    {
        this.MachoNet.QueueInputPacket (this, data);
    }

    private void HandleConnectionLost ()
    {
        Log.Fatal ("Lost connection to node {0}, is it down?", this.Session.NodeID);

        // clean up ourselves
        this.Terminated (this);
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

    public void Close ()
    {
        this.Dispose ();
    }
    
    public void Dispose ()
    {
        this.Socket.Close ();
        this.Socket.DataReceived   -= this.HandlePacket;
        this.Socket.Exception      -= this.HandleException;
        this.Socket.ConnectionLost -= this.HandleConnectionLost;
    }
}