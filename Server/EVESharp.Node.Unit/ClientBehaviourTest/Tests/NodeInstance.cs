using System.Collections.Generic;
using System.Net.Http;
using EVESharp.Common.Configuration;
using EVESharp.Database;
using EVESharp.Database.Account;
using EVESharp.EVE;
using EVESharp.EVE.Accounts;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Messages.Processor;
using EVESharp.EVE.Network;
using EVESharp.EVE.Network.Messages;
using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Notifications;
using EVESharp.Node.Accounts;
using EVESharp.Node.Configuration;
using EVESharp.Node.Server.Node.Messages;
using EVESharp.Node.Server.Shared;
using EVESharp.Node.Server.Shared.Helpers;
using EVESharp.Node.Sessions;
using Moq;
using RichardSzalay.MockHttp;
using Serilog.Core;
using MachoNet = EVESharp.Node.Server.Node.MachoNet;

namespace EVESharp.Node.Unit.ClientBehaviourTest.Tests;

public class NodeInstance
{
    private readonly Mock <IDatabase>           mDatabase           = new Mock <IDatabase> ();
    private readonly Mock <Authentication>                mAuthenticationMock = new Mock <Authentication> ();
    private readonly Mock <General>                       mGeneralMock        = new Mock <General> ();
    private readonly Mock <Common.Configuration.MachoNet> mMachoNetMock       = new Mock <Common.Configuration.MachoNet> ();
    private readonly Mock <Cluster>                       mClusterMock        = new Mock <Cluster> ();
    private readonly Mock <INotificationSender>           mNotificationSender = new Mock <INotificationSender> ();
    private readonly Mock <IItems>                        mItems              = new Mock <IItems> ();
    private readonly Mock <ISolarSystems>                 mSolarSystems       = new Mock <ISolarSystems> ();
    private readonly MockHttpMessageHandler               mHttpMessageHandler = new MockHttpMessageHandler ();

    public  IMachoNet                              MachoNet         { get; }
    public  TransportManager                       TransportManager { get; }
    public  SessionManager                         SessionManager   { get; }
    public  ClusterManager                         ClusterManager   { get; }
    private SynchronousProcessor <LoginQueueEntry> mLoginProcessor;
    private SynchronousProcessor <MachoMessage>    mMessageProcessor;
    private ProxyInstance                          mProxyInstance;
    
    public NodeInstance (ProxyInstance proxy)
    {
        this.mProxyInstance = proxy;
        // setup requests mocks
        this.mHttpMessageHandler
            .Expect (HttpMethod.Post, "http://noderequest/Nodes/register")
            .WithFormData (new Dictionary <string, string> ()
            {
                {"port", "26000"},
                {"role", "server"}
            })
            .Respond ("application/json", "{\"nodeId\": 15,\"address\": \"nodeaddress\", \"startupTime\": 0, \"timeInterval\": 500}");
        // startupTime and timerInterval do not really matter as nodes do not handle any timing stuff
        this.mHttpMessageHandler
            .Expect (HttpMethod.Get, "http://noderequest/Nodes/proxies")
            .Respond ("application/json", "[{\"nodeID\": 65535}]");
        // 65535 should be proxy node
        this.mHttpMessageHandler
            .Expect (HttpMethod.Get, "http://noderequest/Nodes/node/65535")
            .Respond ("application/json", "{\"ip\": \"proxyaddress\", \"port\": 26000, \"role\": \"proxy\"}");
        // open node connection request to obtain destination address
        
        // setup authentication configuration
        this.mAuthenticationMock.SetupGet (x => x.Autoaccount).Returns (false);
        this.mAuthenticationMock.SetupGet (x => x.MessageType).Returns (AuthenticationMessageType.NoMessage);
        this.mAuthenticationMock.SetupGet (x => x.Role).Returns ((long) Roles.ROLE_PLAYER);
        
        // setup machonet configuration
        this.mMachoNetMock.SetupGet (x => x.Mode).Returns (MachoNetMode.Server);
        this.mMachoNetMock.SetupGet (x => x.Port).Returns (26000);
        
        // setup cluster configuration
        this.mClusterMock.SetupGet (x => x.OrchestatorURL).Returns ("http://noderequest");
        
        // setup general configuration
        this.mGeneralMock.SetupGet (x => x.Authentication).Returns (this.mAuthenticationMock.Object);
        this.mGeneralMock.SetupGet (x => x.MachoNet).Returns (this.mMachoNetMock.Object);
        this.mGeneralMock.SetupGet (x => x.Cluster).Returns (this.mClusterMock.Object);

        this.mLoginProcessor = new SynchronousProcessor <LoginQueueEntry> (
            new LoginQueue (this.mDatabase.Object, this.mAuthenticationMock.Object, Logger.None)
        );
        
        // transport manager
        this.TransportManager = new TransportManager (this.mHttpMessageHandler.ToHttpClient(), Logger.None);
        // setup the listener so we can keep track of the socket
        this.TransportManager.OnNewTransportOpen += (socket) =>
        {
            // transport open, link it into the machoNet input
            TestEveClientSocket proxySocket = (this.mProxyInstance.TransportManager.ServerTransport as TestMachoServerTransport).SimulateNewConnection ();
            // link sockets
            proxySocket.DataSent += (data) =>
            {
                socket.SimulateDataReceived (data);
            };
            socket.DataSent += (data) =>
            {
                proxySocket.SimulateDataReceived (data);
            };
        };
        
        this.MachoNet = new MachoNet  (
            this.mDatabase.Object, this.TransportManager, this.mLoginProcessor, this.mGeneralMock.Object, Logger.None
        );
        // session manager
        this.SessionManager = new SessionManager (this.TransportManager, this.MachoNet);
        // message processor
        this.mMessageProcessor = new SynchronousProcessor <MachoMessage> (
            new MessageQueue (
                this.MachoNet,
                Logger.None,
                this.mNotificationSender.Object,
                this.mItems.Object,
                this.mSolarSystems.Object,
                null,
                null,
                Mock.Of <IRemoteServiceManager> (),
                new PacketCallHelper (this.MachoNet),
                this.SessionManager
            )
        );
        // cluster manager
        this.ClusterManager = new ClusterManager (
            this.MachoNet,
            this.TransportManager,
            this.mHttpMessageHandler.ToHttpClient (),
            Mock.Of <ITimers> (),
            Logger.None
        );
    }

    public void Initialize ()
    {
        this.MachoNet.Initialize ();
        this.MachoNet.MessageProcessor = this.mMessageProcessor;
        
        // register the node in the cluster
        this.ClusterManager.RegisterNode ().Wait ();;
        // connect to the proxy
        this.ClusterManager.EstablishConnectionWithProxies ().Wait ();;
    }

    /// <summary>
    /// Performs verifications for this instance
    /// </summary>
    public void Verify ()
    {
        // ensure all the requests were hit
        this.mHttpMessageHandler.VerifyNoOutstandingExpectation ();
    }
}