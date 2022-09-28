using System;
using System.Collections.Generic;
using System.Net.Http;
using EVESharp.Common.Configuration;
using EVESharp.Database;
using EVESharp.EVE;
using EVESharp.EVE.Accounts;
using EVESharp.EVE.Data.Account;
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
using MachoNet = EVESharp.Node.Server.Proxy.MachoNet;

namespace EVESharp.Node.Unit.ClientBehaviourTest.Tests;

public class ProxyInstance
{
    private readonly Mock <IDatabaseConnection>             mDatabase           = new Mock <IDatabaseConnection> ();
    private readonly Mock <Authentication>                  mAuthenticationMock = new Mock <Authentication> ();
    private readonly Mock <General>                         mGeneralMock        = new Mock <General> ();
    private readonly Mock <Common.Configuration.MachoNet>   mMachoNetMock       = new Mock <Common.Configuration.MachoNet> ();
    private readonly Mock <Cluster>                         mClusterMock        = new Mock <Cluster> ();
    private readonly Mock <INotificationSender>             mNotificationSender = new Mock <INotificationSender> ();
    private readonly Mock <IItems>                          mItems              = new Mock <IItems> ();
    private readonly Mock <ISolarSystems>                   mSolarSystems       = new Mock <ISolarSystems> ();
    private readonly Mock <ITimers>                         mTimers             = new Mock <ITimers> ();
    private readonly MockHttpMessageHandler                 mHttpMessageHandler = new MockHttpMessageHandler ();
    private readonly SynchronousProcessor <MachoMessage>    mMessageProcessor;
    
    public           IMachoNet                              MachoNet         { get; }
    public           TransportManager                       TransportManager { get; }
    public           SessionManager                         SessionManager   { get; }
    public           ClusterManager                         ClusterManager   { get; }
    public           SynchronousProcessor <LoginQueueEntry> LoginProcessor   { get; }

    public ProxyInstance ()
    {
        this.mHttpMessageHandler
            .Expect (HttpMethod.Post, "http://proxyrequest/Nodes/register")
            .WithFormData (new Dictionary <string, string> ()
            {
                {"port", "26000"},
                {"role", "proxy"}
            })
            .Respond ("application/json", "{\"nodeId\": 65535, \"address\": \"proxyaddress\", \"startupTime\": 0, \"timeInterval\": 500}");
        // 65535 will be the proxy id to make things easier
        this.mHttpMessageHandler
            .Expect (HttpMethod.Get, "http://proxyrequest/Nodes/nodeaddress")
            .Respond ("application/json", "{\"nodeID\": 15, \"role\": \"server\"}");
        // 
        // setup authentication configuration
        this.mAuthenticationMock.SetupGet (x => x.Autoaccount).Returns (false);
        this.mAuthenticationMock.SetupGet (x => x.MessageType).Returns (AuthenticationMessageType.NoMessage);
        this.mAuthenticationMock.SetupGet (x => x.Role).Returns ((long) Roles.ROLE_PLAYER);
        
        // setup machonet configuration
        this.mMachoNetMock.SetupGet (x => x.Mode).Returns (MachoNetMode.Proxy);
        this.mMachoNetMock.SetupGet (x => x.Port).Returns (26000);
        
        // setup cluster configuration
        this.mClusterMock.SetupGet (x => x.OrchestatorURL).Returns ("http://proxyrequest");
        
        // setup general configuration
        this.mGeneralMock.SetupGet (x => x.Authentication).Returns (this.mAuthenticationMock.Object);
        this.mGeneralMock.SetupGet (x => x.MachoNet).Returns (this.mMachoNetMock.Object);
        this.mGeneralMock.SetupGet (x => x.Cluster).Returns (this.mClusterMock.Object);

        this.LoginProcessor = new SynchronousProcessor <LoginQueueEntry> (
            new LoginQueue (this.mDatabase.Object, this.mAuthenticationMock.Object, Logger.None)
        );
        // setup timer mock
        this.mTimers
            .Setup (
                x => x.EnqueueTimer <It.IsAnyType> (
                    It.IsAny <TimeSpan> (),
                    It.IsAny <Action<It.IsAnyType>> (),
                    It.IsAny <It.IsAnyType> ()
                )
            )
            .Returns (() => null)
            .Verifiable ();
        // transport manager
        this.TransportManager = new TransportManager (this.mHttpMessageHandler.ToHttpClient(), Logger.None);
        this.MachoNet = new MachoNet  (
            this.TransportManager, this.LoginProcessor, this.mGeneralMock.Object, Logger.None, this.mDatabase.Object
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
            this.mTimers.Object,
            Logger.None
        );
    }

    public void Initialize ()
    {
        this.MachoNet.Initialize ();
        this.MachoNet.MessageProcessor = this.mMessageProcessor;
        
        // register the node in the cluster
        this.ClusterManager.RegisterNode ().Wait ();
    }

    /// <summary>
    /// Performs verifications for this instance
    /// </summary>
    public void Verify ()
    {
        // verify the timer calls
        this.mTimers.Verify ();
        // ensure all the requests were hit
        this.mHttpMessageHandler.VerifyNoOutstandingExpectation ();
    }
}