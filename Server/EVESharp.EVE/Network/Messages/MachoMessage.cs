using EVESharp.EVE.Messages;
using EVESharp.EVE.Network.Transports;
using EVESharp.EVE.Types.Network;

namespace EVESharp.EVE.Network.Messages;

public class MachoMessage : IMessage
{
    /// <summary>
    /// The packet to process
    /// </summary>
    public PyPacket Packet { get; init; }
    /// <summary>
    /// The transport that originated the data
    /// </summary>
    public IMachoTransport Transport { get; init; }
}