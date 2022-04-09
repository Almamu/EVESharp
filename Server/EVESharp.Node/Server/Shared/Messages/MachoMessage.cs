using EVESharp.Common.Network.Messages;
using EVESharp.Node.Network;
using EVESharp.Node.Server.Shared.Transports;
using EVESharp.PythonTypes.Types.Network;

namespace EVESharp.Node.Server.Shared.Messages;

public class MachoMessage : IMessage
{
    /// <summary>
    /// The packet to process
    /// </summary>
    public PyPacket Packet { get; init; }
    /// <summary>
    /// The transport that originated the data
    /// </summary>
    public MachoTransport Transport { get; init; }
}