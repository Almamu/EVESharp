using EVESharp.EVE.Network.Transports;

namespace EVESharp.Node.Unit.BehaviourTests.ClientBehaviourTest;

public class TestMachoClientTransport : MachoClientTransport
{
    public TestMachoClientTransport (IMachoTransport source) : base (source) { }
}