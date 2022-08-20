using System;
using EVESharp.Common.Network.Sockets;
using EVESharp.EVE.Sessions; 
using EVESharp.PythonTypes.Types.Primitives;
using Serilog;

namespace EVESharp.EVE.Network.Transports;

public class MachoProxyTransport : IMachoTransport
{
    public Session                        Session          { get; }
    public ILogger                        Log              { get; }
    public IEVESocket                     Socket           { get; }
    public IMachoNet                      MachoNet         { get; }
    public ITransportManager              TransportManager { get; }
    public event Action <IMachoTransport> OnTerminated;
    
    public MachoProxyTransport (IMachoTransport source)
    {
        this.Socket                =  source.Socket;
        this.Log                   =  source.Log;
        this.Session               =  source.Session;
        this.MachoNet              =  source.MachoNet;
        this.TransportManager      =  source.TransportManager;
        
        this.Socket.OnDataReceived   += this.HandlePacket;
        this.Socket.OnException      += this.HandleException;
        this.Socket.OnConnectionLost += this.HandleConnectionLost;
    }

    private void HandlePacket (PyDataType data)
    {
        this.MachoNet.QueueInputPacket (this, data);
    }

    private void HandleConnectionLost ()
    {
        Log.Fatal ("Lost connection to proxy {0}, is it down?", this.Session.NodeID);

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

    public void Close ()
    {
        this.Dispose ();
    }

    public void Dispose ()
    {
        this.Socket.Close ();
        
        this.Socket.OnDataReceived   -= this.HandlePacket;
        this.Socket.OnException      -= this.HandleException;
        this.Socket.OnConnectionLost -= this.HandleConnectionLost;
    }
}