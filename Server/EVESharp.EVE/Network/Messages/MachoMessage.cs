using EVESharp.EVE.Network.Transports;
using EVESharp.PythonTypes.Types.Network;

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
    public MachoTransport Transport { get; init; }
}