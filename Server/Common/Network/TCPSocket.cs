using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Common.Network
{
    public class TCPSocket
    {
        public TCPSocket(ushort socket_port, bool mode_blocking)
        {
            port = socket_port;

            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Blocking = mode_blocking;
        }

        public TCPSocket(Socket from, bool socket_listening)
        {
            sock = from;
            listening = socket_listening;

            if (socket_listening)
            {
                IPEndPoint tmp = (IPEndPoint)sock.LocalEndPoint;
                port = (ushort)tmp.Port;
                ep = tmp;
            }
            else
            {
                IPEndPoint tmp = (IPEndPoint)sock.RemoteEndPoint;
                port = (ushort)tmp.Port;
                ep = tmp;
            }

            if (sock.Blocking == false)
            {
                sock.Blocking = true;
            }

            sock.ReceiveBufferSize = 1024 * 64; // 64 kb
            sock.SendBufferSize = 1024 * 64; // 64 kb
        }

        public bool Connect(string address)
        {
            bool res = true;

            try
            {
                ep = new IPEndPoint(Dns.GetHostEntry(address).AddressList[0], port);
                sock.Connect(ep);
            }
            catch (Exception)
            {
                res = false;
            }

            return res;
        }

        public bool Listen(int backlog)
        {
            bool res = true;

            try
            {
                ep = new IPEndPoint(IPAddress.Any, port);
                sock.Bind(ep);
                sock.Listen(backlog);
                listening = true;
            }
            catch (Exception)
            {
                res = false;
            }

            return res;
        }

        public TCPSocket Accept()
        {
            if (listening == false)
            {
                return null;
            }

            try
            {
                Socket client = sock.Accept();

                if (client == null)
                {
                    return null;
                }

                return new TCPSocket(client, false);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool Close()
        {
            bool res = true;

            try
            {
                sock.Close();
            }
            catch (Exception)
            {
                res = true;
            }

            return res;
        }

        public bool IsConnected()
        {
            if (sock == null)
            {
                return false;
            }

            return sock.Connected;
        }

        public ushort Port()
        {
            return port;
        }

        public int Recv(byte[] data)
        {
            if (sock == null)
            {
                return -1;
            }

            if (IsConnected() == false)
            {
                return -1;
            }

            return sock.Receive(data);
        }

        public int Send(byte[] data)
        {
            if (sock == null)
            {
                return 0;
            }

            return sock.Send(data);
        }

        public string GetAddress()
        {
            IPEndPoint ep = (IPEndPoint)sock.RemoteEndPoint;

            return ep.Address.ToString();
        }

        private EndPoint ep = null;
        private Socket sock = null;
        protected ushort port = 1;
        private bool listening = false;
        public int Available
        {
            get
            {
                return sock.Available;
            }
        }
    }
}
