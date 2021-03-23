using System;
using Common.Logging;
using Common.Network;
using Common.Packets;
using PythonTypes.Types.Primitives;

namespace ClusterController
{
    public class UnauthenticatedConnection : Connection
    {
        private Channel Log { get; set; }

        public UnauthenticatedConnection(EVEClientSocket socket, ConnectionManager connectionManager, Logger logger)
            : base(socket, connectionManager)
        {
            this.Log = logger.CreateLogChannel($"Unauthenticated-{this.Socket.GetRemoteAddress()}");
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
            Log.Debug("Sending LowLevelVersionExchange...");

            LowLevelVersionExchange data = new LowLevelVersionExchange
            {
                codename = Common.Constants.Game.codename,
                birthday = Common.Constants.Game.birthday,
                build = Common.Constants.Game.build,
                machoVersion = Common.Constants.Game.machoVersion,
                version = Common.Constants.Game.version,
                usercount = this.ConnectionManager.ClientsCount,
                region = Common.Constants.Game.region
            };

            this.Socket.Send(data);
        }

        private void ReceiveLowLevelVersionExchangeCallback(PyDataType ar)
        {
            try
            {
                LowLevelVersionExchange exchange = this.CheckLowLevelVersionExchange(ar);

                // TODO: CHECK NETWORK OF THE NODE TO ENSURE UNAUTHORIZED CONNECTIONS DONT REACH A NODE STATE
                if (exchange.isNode)
                    this.ConvertToNodeConnection();
                else
                    this.ConvertToClientConnection();
            }
            catch (Exception e)
            {
                Log.Error($"Exception caught on LowLevelVersionExchange {e.Message}");
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
            Log.Error("Exception detected: ");

            do
            {
                Log.Error(exception.Message);
                Log.Trace(exception.StackTrace);
            } while ((exception = exception.InnerException) != null);
        }
    }
}