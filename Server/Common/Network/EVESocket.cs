using System;
using System.Net;
using System.Net.Sockets;

namespace Common.Network
{
    public abstract class EVESocket
    {
        protected Socket Socket { get; private set; }
        private Action<Exception> mExceptionHandler = null;
        private Action mOnConnectionLost = null;

        public EVESocket()
        {
            this.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.SetTransferBuffers();
            // set a default exception handler
            this.SetExceptionHandler(DefaultExceptionHandler);
        }

        protected EVESocket(Socket socket)
        {
            this.Socket = socket;
            this.SetTransferBuffers();
            // set a default exception handler
            this.SetExceptionHandler(DefaultExceptionHandler);
        }

        private void SetTransferBuffers()
        {
            // 64 kb should be plenty for most situations
            this.Socket.ReceiveBufferSize = 64 * 1024;
            this.Socket.SendBufferSize = 64 * 1024;
        }

        public void SetExceptionHandler(Action<Exception> exceptionHandler)
        {
            this.mExceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        public void SetOnConnectionLostHandler(Action onConnectionLostHandler)
        {
            this.mOnConnectionLost = onConnectionLostHandler;
        }

        protected void HandleException(Exception ex)
        {
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
                        this.mOnConnectionLost?.Invoke();
                        break;
                }
            }
            
            // call the custom exception handler only if the exception cannot be handled by the socket itself
            if (handled == false)
                this.mExceptionHandler.Invoke(ex);
        }

        protected abstract void DefaultExceptionHandler(Exception ex);

        /// <summary>
        /// Disconnects the socket flushing all the input/output buffers
        /// </summary>
        public abstract void GracefulDisconnect();

        public void ForcefullyDisconnect()
        {
            this.Socket.Disconnect(false);
            this.Socket.Shutdown(SocketShutdown.Both);
        }

        public string GetRemoteAddress()
        {
            IPEndPoint endPoint = this.Socket.RemoteEndPoint as IPEndPoint;

            return endPoint.Address.ToString();
        }
    }
}