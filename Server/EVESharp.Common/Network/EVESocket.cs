using System;
using System.Net;
using System.Net.Sockets;

namespace EVESharp.Common.Network;

public abstract class EVESocket
{
    private   Action <Exception> mExceptionHandler;
    private   Action             mOnConnectionLost;
    protected Socket             Socket { get; init; }

    public EVESocket ()
    {
        Socket = new Socket (AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
        // ensure we support both ipv4 and ipv6
        Socket.SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
        this.SetTransferBuffers ();
        // set a default exception handler
        this.SetExceptionHandler (this.DefaultExceptionHandler);
    }

    protected EVESocket (Socket socket)
    {
        Socket = socket;
        // ensure we support both ipv4 and ipv6
        // this.Socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
        this.SetTransferBuffers ();
        // set a default exception handler
        this.SetExceptionHandler (this.DefaultExceptionHandler);
    }

    private void SetTransferBuffers ()
    {
        // 64 kb should be plenty for most situations
        Socket.ReceiveBufferSize = 64 * 1024;
        Socket.SendBufferSize    = 64 * 1024;
    }

    public void SetExceptionHandler (Action <Exception> exceptionHandler)
    {
        this.mExceptionHandler = exceptionHandler ?? throw new ArgumentNullException (nameof (exceptionHandler));
    }

    public void SetOnConnectionLostHandler (Action onConnectionLostHandler)
    {
        this.mOnConnectionLost = onConnectionLostHandler;
    }

    protected void FireOnConnectionLostHandler ()
    {
        try
        {
            this.mOnConnectionLost?.Invoke ();
        }
        catch (Exception)
        {
            // ignored as these are not really important
        }
    }

    protected void HandleException (Exception ex)
    {
        // object disposed exception happens if we try to double-close the connection
        // we don't really care about those, so just mark them as handled
        bool handled = ex is ObjectDisposedException;

        // check for specific disconnection scenarios
        if (ex is SocketException socketException)
            switch (socketException.SocketErrorCode)
            {
                case SocketError.ConnectionAborted:
                case SocketError.ConnectionReset:
                case SocketError.Disconnecting:
                case SocketError.HostUnreachable:
                case SocketError.NetworkDown:
                case SocketError.NetworkReset:
                case SocketError.NetworkUnreachable:
                case SocketError.NoRecovery:
                case SocketError.Fault:
                case SocketError.OperationAborted:
                case SocketError.Shutdown:
                case SocketError.TimedOut:
                case SocketError.ConnectionRefused:
                case SocketError.HostDown:
                case SocketError.NoData:
                case SocketError.NotConnected:
                case SocketError.NotInitialized:
                case SocketError.ProtocolOption:
                case SocketError.HostNotFound:
                    handled = true;
                    this.FireOnConnectionLostHandler ();

                    break;
            }

        // call the custom exception handler only if the exception cannot be handled by the socket itself
        if (handled == false)
            this.mExceptionHandler.Invoke (ex);
    }

    protected abstract void DefaultExceptionHandler (Exception ex);

    /// <summary>
    /// Disconnects the socket flushing all the input/output buffers
    /// </summary>
    public abstract void GracefulDisconnect ();

    public void ForcefullyDisconnect ()
    {
        // disconnect and shutdown the socket
        Socket.Disconnect (false);
        Socket.Shutdown (SocketShutdown.Both);
        // finally close it
        Socket.Close ();
    }

    public string GetRemoteAddress ()
    {
        IPEndPoint endPoint = Socket.RemoteEndPoint as IPEndPoint;

        return endPoint?.Address.ToString ();
    }
}