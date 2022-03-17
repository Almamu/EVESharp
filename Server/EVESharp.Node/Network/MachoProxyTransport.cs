using EVESharp.Common.Logging;
using EVESharp.Common.Network;

namespace EVESharp.Node.Network;

public class MachoProxyTransport : MachoTransport
{
    public MachoProxyTransport(MachoTransport source) : base(source)
    {
    }
}