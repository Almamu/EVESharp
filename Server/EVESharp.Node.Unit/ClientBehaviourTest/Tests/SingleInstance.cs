using System.Net.Http;
using EVESharp.Common.Configuration;
using EVESharp.EVE.Accounts;
using EVESharp.EVE.Data.Account;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Messages.Processor;
using EVESharp.EVE.Network;
using EVESharp.EVE.Network.Messages;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Services;
using EVESharp.Node.Accounts;
using EVESharp.Node.Configuration;
using EVESharp.Node.Server.Shared.Helpers;
using EVESharp.Node.Server.Single;
using EVESharp.Node.Server.Single.Messages;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes.Database;
using EVESharp.PythonTypes.Types.Database;
using Moq;
using Serilog.Core;
using MachoNet = EVESharp.Node.Server.Single.MachoNet;

namespace EVESharp.Node.Unit.ClientBehaviourTest.Tests;

public class SingleInstance
{
    private readonly Mock <IDatabaseConnection>           mDatabase           = new Mock <IDatabaseConnection> ();
    private readonly Mock <Authentication>                mAuthenticationMock = new Mock <Authentication> ();
    private readonly Mock <General>                       mGeneralMock        = new Mock <General> ();
    private readonly Mock <Common.Configuration.MachoNet> mMachoNetMock       = new Mock <Common.Configuration.MachoNet> ();
    private readonly Mock <Cluster>                       mClusterMock        = new Mock <Cluster> ();
    private readonly Mock <INotificationSender>           mNotificationSender = new Mock <INotificationSender> ();
    private readonly Mock <IItems>                        mItems              = new Mock <IItems> ();
    private readonly Mock <ISolarSystems>                 mSolarSystems       = new Mock <ISolarSystems> ();

    public  IMachoNet                              MachoNet       { get; }
    public SynchronousProcessor <LoginQueueEntry> LoginProcessor { get; }
    private SessionManager                         mSessionManager;
    public  SynchronousProcessor <MachoMessage>    MessageProcessor { get; }
    public  TransportManager                       TransportManager { get; }

    public SingleInstance ()
    {
        // setup authentication configuration
        this.mAuthenticationMock.SetupGet (x => x.Autoaccount).Returns (false);
        this.mAuthenticationMock.SetupGet (x => x.MessageType).Returns (AuthenticationMessageType.NoMessage);
        this.mAuthenticationMock.SetupGet (x => x.Role).Returns ((long) Roles.ROLE_PLAYER);
        
        // setup machonet configuration
        this.mMachoNetMock.SetupGet (x => x.Mode).Returns (MachoNetMode.Single);
        this.mMachoNetMock.SetupGet (x => x.Port).Returns (26000);
        
        // setup cluster configuration
        this.mClusterMock.SetupGet (x => x.OrchestatorURL).Returns ("INVALID URL!");
        
        // setup general configuration
        this.mGeneralMock.SetupGet (x => x.Authentication).Returns (this.mAuthenticationMock.Object);
        this.mGeneralMock.SetupGet (x => x.MachoNet).Returns (this.mMachoNetMock.Object);
        this.mGeneralMock.SetupGet (x => x.Cluster).Returns (this.mClusterMock.Object);

        this.LoginProcessor = new SynchronousProcessor <LoginQueueEntry> (
            new LoginQueue (this.mDatabase.Object, this.mAuthenticationMock.Object, Logger.None)
        );
        // transport manager
        this.TransportManager = new TransportManager (new HttpClient (), Logger.None);
        // macho net
        this.MachoNet = new MachoNet (
            this.mDatabase.Object, this.TransportManager, this.LoginProcessor, this.mGeneralMock.Object, Logger.None
        );
        // session manager
        this.mSessionManager = new SessionManager (this.TransportManager, this.MachoNet);
        // message processor
        this.MessageProcessor = new SynchronousProcessor <MachoMessage> (
            new MessageQueue (
                this.MachoNet,
                Logger.None,
                null,
                null,
                Mock.Of <IRemoteServiceManager> (),
                new PacketCallHelper (this.MachoNet),
                this.mNotificationSender.Object,
                this.mItems.Object,
                this.mSolarSystems.Object,
                this.mSessionManager
            )
        );
    }

    public void Initialize ()
    {
        this.MachoNet.Initialize ();
        this.MachoNet.MessageProcessor = this.MessageProcessor;
    }
}