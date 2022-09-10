using System;
using System.Net.Http;
using EVESharp.Common.Logging;
using EVESharp.EVE.Network.Sockets;
using Serilog;

namespace EVESharp.EVE.Network.Transports;

public class MachoServerTransport : EVEListener
{
    public  IMachoNet MachoNet { get; }
    private ILogger   Log      { get; }

    public MachoServerTransport (int port, IMachoNet machoNet, ILogger log) : base (port)
    {
        this.Log           =  log;
        this.MachoNet      =  machoNet;
        ConnectionAccepted += this.ConnectionAcceptedHandler;
        base.Exception   += this.ExceptionHandler;
    }

    private void ConnectionAcceptedHandler (IEVESocket socket)
    {
        this.MachoNet.TransportManager.NewTransport (this.MachoNet, socket);
    }

    private void ExceptionHandler (Exception ex)
    {
        Log.Error ("Exception on server listener: {ex.Message}", ex.Message);
        Log.Error (ex.StackTrace);
    }
}