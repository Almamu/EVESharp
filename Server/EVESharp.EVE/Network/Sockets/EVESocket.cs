using System;
using System.Net;
using System.Net.Sockets;
using EVESharp.Common.Compression;
using EVESharp.Types;
using EVESharp.Types.Serialization;

namespace EVESharp.EVE.Network.Sockets;

public class EVESocket : IEVESocket
{
    private Socket               Socket       { get; }
    private StreamPacketizer     Packetizer   { get; } = new StreamPacketizer ();
    private ReceiveCallbackState ReceiveState { get; } = new ReceiveCallbackState (new byte [64 * 1024]);
    private bool                 IsClosed     { get; set; }
    private bool                 IsDisposed   { get; set; }

    public event Action <PyDataType> DataReceived;
    public event Action              ConnectionLost;
    public event Action <Exception>  Exception;
    public virtual string            RemoteAddress => (this.Socket.RemoteEndPoint as IPEndPoint)?.Address.ToString ();

    public EVESocket ()
    {
        this.Socket = new Socket (AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
        // ensure support for both ipv4 and ipv4
        this.Socket.SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
        // setup transfer buffers
        this.Socket.ReceiveBufferSize = 64 * 1024;
        this.Socket.SendBufferSize    = 64 * 1024;
    }
    
    public EVESocket (Socket socket)
    {
        this.Socket = socket;
        // setup transfer buffers
        this.Socket.ReceiveBufferSize = 64 * 1024;
        this.Socket.SendBufferSize    = 64 * 1024;
        // setup receive callbacks
        this.SetupReceiveCallback ();
    }

    public void Connect (string address, int port)
    {
        // try to resolve the address to an IP
        if (IPAddress.TryParse (address, out IPAddress ip) == false)
        {
            IPHostEntry entry = Dns.GetHostEntry (address);
            
            foreach (IPAddress tmp in entry.AddressList)
            {
                if (tmp.AddressFamily != AddressFamily.InterNetwork)
                    continue;

                ip = tmp;
                break;
            }
        }
        
        // connect to the server
        this.Socket.Connect (new IPEndPoint (ip, port));
        // setup receive callbacks
        this.SetupReceiveCallback ();
    }

    private void SetupReceiveCallback ()
    {
        this.Socket.BeginReceive (this.ReceiveState.Buffer, 0, this.ReceiveState.Buffer.Length, SocketFlags.None, this.ReceiveCallback, this.ReceiveState);
    }

    private void ProcessInputData ()
    {
        // no data received callback means the data cannot be processed yet
        if (this.DataReceived is null)
            return;
        
        while (this.Packetizer.PacketCount > 0)
        {
            try
            {
                PyDataType packet = Unmarshal.ReadFromByteArray (this.Packetizer.PopItem ());

                this.OnDataReceived (packet);
            }
            catch (Exception e)
            {
                this.HandleException (e);
            }
        }
    }

    protected void OnDataReceived (PyDataType data)
    {
        if (this.DataReceived is not null)
            this.DataReceived (data);
    }

    private void ReceiveCallback (IAsyncResult ar)
    {
        try
        {
            ReceiveCallbackState state = ar.AsyncState as ReceiveCallbackState;

            state.Received = this.Socket.EndReceive (ar);
            
            // receiving 0 bytes means the socket was closed on the other end
            if (state.Received == 0)
            {
                this.Close ();
                return;
            }

            // process the input data
            this.Packetizer.QueuePackets (state.Buffer, state.Received);
            this.Packetizer.ProcessPackets ();
            // notify of any pending packets
            this.ProcessInputData ();
        }
        catch (Exception e)
        {
            this.HandleException (e);
        }
        
        if (this.Socket.Connected == true)
            // start receiving again
            this.SetupReceiveCallback ();
    }

    private void SendCallback (IAsyncResult ar)
    {
        try
        {
            this.Socket.EndSend (ar);
        }
        catch (Exception e)
        {
            this.Exception?.Invoke (e);
        }
    }

    private void HandleException (Exception ex)
    {
        // object disposed exception happens if we try to double-close the connection
        // we don't really care about those, so just mark them as handled
        bool handled = false;

        // check for specific disconnection scenarios
        if (ex is SocketException socketException)
        {
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
                    this.ConnectionLost?.Invoke ();
                    break;
            }
        }

        // call the custom exception handler only if the exception cannot be handled by the socket itself
        if (handled == false)
            this.Exception?.Invoke (ex);
    }

    public virtual void Send (PyDataType data)
    {
        // convert the data to bytes
        byte [] encodedPacket = Marshal.ToByteArray (data);
        // compress the packet if required
        if (encodedPacket.Length > Common.Constants.Network.MAX_PACKET_SIZE)
            encodedPacket = ZlibHelper.Compress (encodedPacket);
        
        // generate the final buffer
        byte [] packetBuffer = new byte [encodedPacket.Length + sizeof (int)];
        
        // write the packet size and the buffer to the final buffer
        Buffer.BlockCopy (BitConverter.GetBytes (encodedPacket.Length), 0, packetBuffer, 0,            sizeof (int));
        Buffer.BlockCopy (encodedPacket,                                0, packetBuffer, sizeof (int), encodedPacket.Length);

        // finally start actually sending the data
        this.Socket.BeginSend (packetBuffer, 0, packetBuffer.Length, SocketFlags.None, this.SendCallback, this);
    }

    public void Close ()
    {
        if (this.IsClosed == true)
            return;

        this.IsClosed = true;
        
        this.Socket.Shutdown (SocketShutdown.Both);
        this.Socket.Disconnect (false);
        this.Socket.Close ();
        
        // clear all the callbacks
        this.DataReceived   = null;
        this.Exception      = null;
        this.ConnectionLost = null;
    }
    
    public void Dispose ()
    {
        if (this.IsDisposed == true)
            return;

        this.IsDisposed = true;
        this.Socket.Dispose ();
    }
}