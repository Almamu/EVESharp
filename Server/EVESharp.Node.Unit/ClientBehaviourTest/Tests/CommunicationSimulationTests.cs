using System.Net.Http;
using EVESharp.Common.Configuration;
using EVESharp.EVE.Accounts;
using EVESharp.EVE.Data.Account;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Messages.Processor;
using EVESharp.EVE.Network;
using EVESharp.EVE.Network.Messages;
using EVESharp.EVE.Notifications;
using EVESharp.Node.Accounts;
using EVESharp.Node.Configuration;
using EVESharp.Node.Server.Shared.Helpers;
using EVESharp.Node.Server.Single.Messages;
using EVESharp.Node.Sessions;
using EVESharp.Node.Unit.Utils;
using HarmonyLib;
using Moq;
using NUnit.Framework;
using Serilog.Core;
using MachoNet = EVESharp.Node.Server.Single.MachoNet;

namespace EVESharp.Node.Unit.ClientBehaviourTest.Tests;

[TestFixture]
public class CommunicationSimulationTests
{
    private readonly Harmony mHarmony = new Harmony ("SingleInstanceTests");
    
    [SetUp]
    public void SetUp ()
    {
        this.mHarmony.Setup (typeof (HarmonyPatches));
    }

    [TearDown]
    public void TearDown ()
    {
        this.mHarmony.UnpatchAll ();
    }

    [Test]
    public void MultipleMachoNetStartupTest ()
    {
        ProxyInstance proxy = new ProxyInstance ();
        NodeInstance  node  = new NodeInstance (proxy);
        
        proxy.Initialize ();
        node.Initialize ();
        
        // verify mocks as they should be done by now, this ensures the initialization happened right
        proxy.Verify ();
        node.Verify ();
        
        // create the LoginProcess handler
        ClientInstance clientInstance = new ClientInstance (
            (proxy.TransportManager.ServerTransport as TestMachoServerTransport).SimulateNewConnection (),
            proxy.MachoNet
        );
        // process the login queue now
        proxy.LoginProcessor.ProcessNextMessage ();
        // ensure the client instance finished initialization
        clientInstance.Verify ();
    }

    [Test]
    public void SingleMachoNetStartupTest ()
    {
        SingleInstance instance = new SingleInstance ();
        
        instance.Initialize ();
        
        // create the LoginProcess handler
        ClientInstance clientInstance = new ClientInstance (
            (instance.TransportManager.ServerTransport as TestMachoServerTransport).SimulateNewConnection (),
            instance.MachoNet
        );
        // process the login queue now
        instance.LoginProcessor.ProcessNextMessage ();
        clientInstance.Verify ();
    }
}