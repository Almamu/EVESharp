using EVESharp.EVE.Network;
using EVESharp.EVE.Network.Transports;
using EVESharp.EVE.Packets;
using EVESharp.EVE.Sessions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;

namespace EVESharp.Node.Sessions;

public class SessionManager : EVE.Sessions.SessionManager
{
    private IMachoNet        MachoNet         { get; }
    private ITransportManager TransportManager { get; }

    public SessionManager (ITransportManager transportManager, IMachoNet machoNet)
    {
        TransportManager = transportManager;
        MachoNet         = machoNet;

        // register events
        TransportManager.OnTransportRemoved += this.OnTransportClosed;
        TransportManager.OnClientResolved   += this.OnClientResolved;
    }

    public override void InitializeSession (Session session)
    {
        // add the session to the list first
        this.RegisterSession (session);

        // build the initial state notification
        PyPacket packet = new PyPacket (PyPacket.PacketType.SESSIONINITIALSTATENOTIFICATION)
        {
            Source      = new PyAddressNode (MachoNet.NodeID),
            Destination = new PyAddressClient (session.UserID, 0),
            UserID      = session.UserID,
            Payload     = new SessionInitialStateNotification {Session = session},
            OutOfBounds = new PyDictionary {["channel"]                = "sessionchange"}
        };
        // send the packet to the player
        MachoNet.QueueOutputPacket (packet);
    }

    /// <summary>
    /// Updates sessions based on the idType and id as criteria
    /// </summary>
    /// <param name="idType"></param>
    /// <param name="id"></param>
    /// <param name="newValues">The new values for the session</param>
    public override void PerformSessionUpdate (string idType, int id, Session newValues)
    {
        switch (MachoNet.Mode)
        {
            case RunMode.Proxy:
            case RunMode.Single:
                this.PerformSessionUpdateForProxy (idType, id, newValues);
                break;

            case RunMode.Server:
                this.PerformSessionUpdateForNode (idType, id, newValues);
                break;

        }
    }

    private void PerformSessionUpdateForProxy (string idType, int id, Session newValues)
    {
        // find all sessions
        foreach (Session session in this.FindSession (idType, id))
        {
            SessionChange delta = UpdateAttributes (session, newValues);

            // no difference means no notification
            if (delta.Count == 0)
                return;

            SessionChangeNotification scn = new SessionChangeNotification
            {
                Changes         = delta,
                NodesOfInterest = session.NodesOfInterest
            };

            // difference noticed, send session change to relevant nodes and player
            PyPacket nodePacket = new PyPacket (PyPacket.PacketType.SESSIONCHANGENOTIFICATION)
            {
                Source      = new PyAddressNode (MachoNet.NodeID),
                Destination = new PyAddressBroadcast (session.NodesOfInterest, "nodeid"),
                Payload     = scn,
                UserID      = session.UserID,
                OutOfBounds = new PyDictionary
                {
                    ["channel"]     = "sessionchange",
                    ["characterID"] = session.CharacterID
                }
            };

            PyPacket clientPacket = new PyPacket (PyPacket.PacketType.SESSIONCHANGENOTIFICATION)
            {
                Source      = new PyAddressNode (MachoNet.NodeID),
                Destination = new PyAddressClient (session.UserID),
                Payload     = scn,
                UserID      = session.UserID,
                OutOfBounds = new PyDictionary {["channel"] = "sessionchange"}
            };

            MachoNet.QueueOutputPacket (nodePacket);
            MachoNet.QueueOutputPacket (clientPacket);
        }
    }

    private void PerformSessionUpdateForNode (string idType, int id, Session newValues)
    {
        PyPacket packet = new PyPacket (PyPacket.PacketType.NOTIFICATION)
        {
            Source      = new PyAddressNode (MachoNet.NodeID),
            Destination = new PyAddressAny (0),
            Payload = new PyTuple (2)
            {
                [0] = "UpdateSessionAttributes",
                [1] = new PyTuple (3)
                {
                    [0] = idType,
                    [1] = id,
                    [2] = newValues
                }
            }
        };

        // notify all proxies
        foreach ((long _, MachoTransport transport) in MachoNet.TransportManager.ProxyTransports)
            transport.Socket.Send (packet);
    }

    public new void FreeSession (Session session)
    {
        base.FreeSession (session);
    }

    private void OnTransportClosed (object sender, MachoTransport transport)
    {
        if (transport is MachoClientTransport)
            this.FreeSession (transport.Session);
    }

    private void OnClientResolved (object sender, MachoClientTransport transport)
    {
        this.InitializeSession (transport.Session);
    }
}