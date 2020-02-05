using System;
using Common;
using Common.Network;
using Common.Packets;
using Marshal;

namespace ClusterControler
{
    public class UnauthenticatedConnection : Connection
    {
        private AsyncCallback mReceive = null;

        public UnauthenticatedConnection(EVEClientSocket socket, ConnectionManager connectionManager)
            : base(socket, connectionManager)
        {
            // set the new exception handler
            this.Socket.SetExceptionHandler(ExceptionHandler);
            // send the low level version exchange
            this.SendLowLevelVersionExchange();
            // setup the first async handler (for the low level version exchange)
            this.Socket.SetReceiveCallback(ReceiveLowLevelVersionExchangeCallback);
        }

        protected override void OnConnectionLost()
        {
            // unregister this connection
            this.ConnectionManager.RemoveUnauthenticatedConnection(this);
            // close the socket forcefully
            this.Socket.ForcefullyDisconnect();
        }

        private void SendLowLevelVersionExchange()
        {
            Log.Debug("Client", "Sending LowLevelVersionExchange...");

            LowLevelVersionExchange data = new LowLevelVersionExchange();

            data.codename = Common.Constants.Game.codename;
            data.birthday = Common.Constants.Game.birthday;
            data.build = Common.Constants.Game.build;
            data.machoVersion = Common.Constants.Game.machoVersion;
            data.version = Common.Constants.Game.version;
            data.usercount = this.ConnectionManager.ClientsCount;
            data.region = Common.Constants.Game.region;

            this.Socket.Send(data.Encode());
        }

        private void ReceiveLowLevelVersionExchangeCallback(PyObject ar)
        {
            try
            {
                LowLevelVersionExchange exchange = this.CheckLowLevelVersionExchange(ar);
                
                // TODO: CHECK NETWORK OF THE NODE TO ENSURE UNAUTHORIZED CONNECTIONS DONT REACH A NODE STATE
                if(exchange.isNode)
                    this.ConvertToNodeConnection();
                else
                    this.ConvertToClientConnection();
            }
            catch (Exception e)
            {
                Log.Error("LowLevelVersionExchange", e.Message);
                throw;
            }
        }

        public void ConvertToClientConnection()
        {
            // disable receive callback, this way the rest of the received data is pending to be processed
            this.Socket.SetReceiveCallback(null);
            this.ConnectionManager.RemoveUnauthenticatedConnection(this);
            this.ConnectionManager.AddUnauthenticatedClientConnection(this.Socket);
        }

        public void ConvertToNodeConnection()
        {
            // disable receive callback, this way the rest of the received data is pending to be processed
            this.Socket.SetReceiveCallback(null);
            this.ConnectionManager.RemoveUnauthenticatedConnection(this);
            this.ConnectionManager.AddNodeConnection(this.Socket);
        }

        protected void ExceptionHandler(Exception exception)
        {
            Log.Error("UnauthenticatedConnection", exception.Message);
        }
    }
}