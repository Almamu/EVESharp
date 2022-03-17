using EVESharp.Common.Logging;
using EVESharp.Common.Network;

namespace EVESharp.Node.Network;

public class MachoNodeTransport : MachoTransport
{
    public MachoNodeTransport(MachoTransport source) : base(source)
    {
    }
}