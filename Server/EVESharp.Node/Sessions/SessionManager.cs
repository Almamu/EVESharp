using System.Collections.Generic;
using EVESharp.EVE.Packets;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Configuration;
using EVESharp.Node.Network;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using MachoNet = EVESharp.Node.Network.MachoNet;

namespace EVESharp.Node.Sessions;

public class SessionManager : EVE.Sessions.SessionManager
{
    private General Configuration { get; }
    private MachoNet MachoNet { get; }
    
    public SessionManager(General configuration, MachoNet machoNet)
    {
        this.Configuration = configuration;
        this.MachoNet = machoNet;
        this.MachoNet.SessionManager = this;
    }

    public void InitializeSession(Session session)
    {
        // add the session to the list first
        this.RegisterSession(session);
        
        // build session initialization packet
        SessionInitialStateNotification notif = new SessionInitialStateNotification()
        {
            Session = session
        };
        // build the initial state notification
        PyPacket packet = new PyPacket(PyPacket.PacketType.SESSIONINITIALSTATENOTIFICATION)
        {
            Source = new PyAddressNode(this.MachoNet.Container.NodeID),
            Destination = new PyAddressClient(session.UserID, 0),
            UserID = session.UserID,
            Payload = notif,
            OutOfBounds = new PyDictionary
            {
                ["channel"] = "sessionchange"
            }
        };
        // send the packet to the player
        this.MachoNet.QueuePacket(packet);
    }
    
    /// <summary>
    /// Updates sessions based on the idType and id as criteria
    /// </summary>
    /// <param name="idType"></param>
    /// <param name="id"></param>
    /// <param name="newValues">The new values for the session</param>
    public void PerformSessionUpdate(string idType, int id, Session newValues)
    {
        switch (this.Configuration.MachoNet.Mode)
        {
            case MachoNetMode.Proxy:
            case MachoNetMode.Single:
                this.PerformSessionUpdateForProxy(idType, id, newValues);
                break;
            case MachoNetMode.Server:
                this.PerformSessionUpdateForNode(idType, id, newValues);
                break;
        }
    }

    private void PerformSessionUpdateForProxy(string idType, int id, Session newValues)
    {
        // find all sessions
        foreach (Session session in this.FindSession(idType, id))
        {
            SessionChange delta = UpdateAttributes(session, newValues);

            // no difference means no notification
            if (delta.Count == 0)
                return;

            SessionChangeNotification scn = new SessionChangeNotification()
            {
                Changes = delta,
                NodesOfInterest = session.NodesOfInterest
            };
            
            // difference noticed, send session change to relevant nodes and player
            PyPacket nodePacket = new PyPacket(PyPacket.PacketType.SESSIONCHANGENOTIFICATION)
            {
                Source = new PyAddressNode(this.MachoNet.Container.NodeID),
                Destination = new PyAddressBroadcast(session.NodesOfInterest, "nodeid"),
                Payload = scn,
                UserID = session.UserID,
                OutOfBounds = new PyDictionary()
                {
                    ["channel"] = "sessionchange"
                }
            };

            PyPacket clientPacket = new PyPacket(PyPacket.PacketType.SESSIONCHANGENOTIFICATION)
            {
                Source = new PyAddressNode(this.MachoNet.Container.NodeID),
                Destination = new PyAddressClient(session.UserID),
                Payload = scn,
                UserID = session.UserID,
                OutOfBounds = new PyDictionary()
                {
                    ["channel"] = "sessionchange"
                }
            };

            this.MachoNet.QueuePacket(nodePacket);
            this.MachoNet.QueuePacket(clientPacket);
        }
    }

    private void PerformSessionUpdateForNode(string idType, int id, Session newValues)
    {
        PyPacket packet = new PyPacket(PyPacket.PacketType.NOTIFICATION)
        {
            Source = new PyAddressNode(this.MachoNet.Container.NodeID),
            Destination = new PyAddressAny(0),
            Payload = new PyTuple(2) {[0] = "UpdateSessionAttributes", [1] = new PyTuple(3) {[0] = idType, [1] = id, [2] = newValues}}
        };
        
        // notify all proxies
        foreach ((long _, MachoTransport transport) in this.MachoNet.Transport.ProxyTransports)
            transport.Socket.Send(packet);
    }

    public new void FreeSession(Session session)
    {
        base.FreeSession(session);
    }
}