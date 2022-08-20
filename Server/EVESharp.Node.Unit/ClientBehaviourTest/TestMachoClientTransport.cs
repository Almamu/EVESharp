using EVESharp.EVE.Network.Transports;

namespace EVESharp.Node.Unit.ClientBehaviourTest;

public class TestMachoClientTransport : MachoClientTransport
{
    public TestMachoClientTransport (IMachoTransport source) : base (source) { }
}