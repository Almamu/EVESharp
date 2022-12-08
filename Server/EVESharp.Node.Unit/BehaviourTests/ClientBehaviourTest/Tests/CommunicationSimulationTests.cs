using NUnit.Framework;

namespace EVESharp.Node.Unit.BehaviourTests.ClientBehaviourTest.Tests;

[TestFixture]
public class CommunicationSimulationTests
{
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