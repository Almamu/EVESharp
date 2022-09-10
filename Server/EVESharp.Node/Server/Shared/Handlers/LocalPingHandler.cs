using System;
using EVESharp.EVE.Network;
using EVESharp.EVE.Network.Messages;
using EVESharp.EVE.Types.Network;
using EVESharp.Types.Collections;

namespace EVESharp.Node.Server.Shared.Handlers;

public class LocalPingHandler
{
    public IMachoNet MachoNet { get; }

    public LocalPingHandler (IMachoNet machoNet)
    {
        MachoNet = machoNet;
    }

    public void HandlePingReq (MachoMessage machoMessage)
    {
        // alter package to include the times the data
        PyAddressClient source = machoMessage.Packet.Source as PyAddressClient;

        // this time should come from the stream packetizer or the socket itself
        // but there's no way we're adding time tracking for all the goddamned packets
        // so this should be sufficient
        PyTuple handleMessage = new PyTuple (3)
        {
            [0] = DateTime.UtcNow.ToFileTime (),
            [1] = DateTime.UtcNow.ToFileTime (),
            [2] = "proxy::handle_message"
        };

        PyTuple writing = new PyTuple (3)
        {
            [0] = DateTime.UtcNow.ToFileTime (),
            [1] = DateTime.UtcNow.ToFileTime (),
            [2] = "proxy::writing"
        };

        // this time should come from the stream packetizer or the socket itself
        // but there's no way we're adding time tracking for all the goddamned packets
        // so this should be sufficient
        PyTuple proxyHandleMessage = new PyTuple (3)
        {
            [0] = DateTime.UtcNow.ToFileTime (),
            [1] = DateTime.UtcNow.ToFileTime (),
            [2] = "proxy::handle_message"
        };

        PyTuple proxyWriting = new PyTuple (3)
        {
            [0] = DateTime.UtcNow.ToFileTime (),
            [1] = DateTime.UtcNow.ToFileTime (),
            [2] = "proxy::writing"
        };

        // this time should come from the stream packetizer or the socket itself
        // but there's no way we're adding time tracking for all the goddamned packets
        // so this should be sufficient
        PyTuple serverHandleMessage = new PyTuple (3)
        {
            [0] = DateTime.UtcNow.ToFileTime (),
            [1] = DateTime.UtcNow.ToFileTime (),
            [2] = "server::handle_message"
        };

        PyTuple serverTurnaround = new PyTuple (3)
        {
            [0] = DateTime.UtcNow.ToFileTime (),
            [1] = DateTime.UtcNow.ToFileTime (),
            [2] = "server::turnaround"
        };

        // TODO: SEND THESE TO A RANDOM NODE THAT IS NOT US!
        (machoMessage.Packet.Payload [0] as PyList)?.Add (handleMessage);
        (machoMessage.Packet.Payload [0] as PyList)?.Add (writing);
        (machoMessage.Packet.Payload [0] as PyList)?.Add (proxyHandleMessage);
        (machoMessage.Packet.Payload [0] as PyList)?.Add (proxyWriting);
        (machoMessage.Packet.Payload [0] as PyList)?.Add (serverHandleMessage);
        (machoMessage.Packet.Payload [0] as PyList)?.Add (serverTurnaround);

        // change to a response
        machoMessage.Packet.Type = PyPacket.PacketType.PING_RSP;

        // switch source and destination
        machoMessage.Packet.Source      = machoMessage.Packet.Destination;
        machoMessage.Packet.Destination = source;

        // queue the packet back
        MachoNet.QueueOutputPacket (machoMessage.Transport, machoMessage.Packet);
    }
}