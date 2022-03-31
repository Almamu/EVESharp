using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EVESharp.Common.Network;
using EVESharp.EVE;
using EVESharp.EVE.Packets;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.Accounts;
using EVESharp.Node.Configuration;
using EVESharp.Node.Exceptions.corpStationMgr;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Network
{
    public class MachoClientTransport : MachoTransport
    {
        public MachoClientTransport(MachoTransport source) : base(source)
        {
            // finally assign the correct packet handler
            this.Socket.SetReceiveCallback(ReceiveNormalPacket);
            this.Socket.SetExceptionHandler(HandleException);
            this.Socket.SetOnConnectionLostHandler(HandleConnectionLost);
        }

        private void HandleConnectionLost()
        {
            Log.Fatal("Client {0} lost connection to the server", this.Session.UserID);
            
            // clean up ourselves
            this.MachoNet.OnTransportTerminated(this);
            // tell all the nodes that we're dead now
            this.MachoNet.QueueOutputPacket(
                new PyPacket(PyPacket.PacketType.NOTIFICATION)
                {
                    Source = new PyAddressAny(0),
                    Destination = new PyAddressBroadcast(this.Session.NodesOfInterest, "nodeid"),
                    Payload = new PyTuple(2) {[0] = "ClientHasDisconnected", [1] = new PyTuple(1) { [0] = this.Session.UserID }},
                    UserID = this.Session.UserID,
                    OutOfBounds = new PyDictionary() {["Session"] = this.Session}
                }
            );
        }
        
        private void HandleException(Exception ex)
        {
            Log.Error("Exception detected: ");

            do
            {
                Log.Error("{0}\n{1}", ex.Message, ex.StackTrace);
            } while ((ex = ex.InnerException) != null);
        }

        private void ReceiveNormalPacket(PyDataType packet)
        {
            if (packet is PyObject)
                throw new Exception("Got exception from client");

            PyPacket pyPacket = packet;
            
            // replace the address if specific situations occur (why is CCP doing it like this?)
            if (pyPacket.Type == PyPacket.PacketType.NOTIFICATION && pyPacket.Source is PyAddressNode)
                pyPacket.Source = new PyAddressClient(this.Session.UserID);
            // ensure the source address is right as it cannot be trusted
            if (pyPacket.Source is not PyAddressClient source)
                throw new Exception("Received a packet from client without a source client address");
            if (pyPacket.UserID != this.Session.UserID)
                throw new Exception("Received a packet coming from a client trying to spoof it's userID");
            
            // ensure the clientId is set in the PyAddressClient
            source.ClientID = this.Session.UserID;
            
            // queue the input packet into machoNet so it handles it
            this.MachoNet.QueueInputPacket(this, pyPacket);
        }
    }    
}