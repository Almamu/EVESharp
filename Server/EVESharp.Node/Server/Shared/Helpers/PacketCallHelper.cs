using EVESharp.EVE.Network;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Types.Network;
using EVESharp.Node.Services;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Node.Server.Shared.Helpers;

public class PacketCallHelper
{
    private IMachoNet MachoNet { get; }

    public PacketCallHelper (IMachoNet machoNet)
    {
        MachoNet = machoNet;
    }

    /// <summary>
    /// Sends a provisional response to the given call
    /// </summary>
    /// <param name="answerTo"></param>
    /// <param name="response"></param>
    public void SendProvisionalResponse (CallInformation answerTo, ProvisionalResponse response)
    {
        // ensure destination has clientID in it
        PyAddressClient source = answerTo.Source as PyAddressClient;

        source.ClientID = answerTo.Session.UserID;

        PyPacket result = new PyPacket (PyPacket.PacketType.CALL_RSP)
        {
            // switch source and dest
            Source      = answerTo.Destination,
            Destination = source,
            UserID      = source.ClientID,
            Payload     = new PyTuple (1) {[0] = 0},
            OutOfBounds = new PyDictionary
            {
                ["provisional"] = new PyTuple (3)
                {
                    [0] = response.Timeout, // macho timeout in seconds
                    [1] = response.EventID,
                    [2] = response.Arguments
                }
            }
        };

        MachoNet.QueueOutputPacket (null, result);
    }

    /// <summary>
    /// Sends a call result to the given call
    /// </summary>
    /// <param name="answerTo"></param>
    /// <param name="content"></param>
    /// <param name="outOfBounds"></param>
    public void SendCallResult (CallInformation answerTo, PyDataType content, PyDictionary outOfBounds = null)
    {
        // ensure destination has clientID in it
        PyAddressClient originalSource = answerTo.Source as PyAddressClient;
        originalSource.ClientID = answerTo.Session.UserID;

        MachoNet.QueueOutputPacket (
            answerTo.Transport,
            new PyPacket (PyPacket.PacketType.CALL_RSP)
            {
                // switch source and dest
                Source      = answerTo.Destination,
                Destination = originalSource,
                UserID      = originalSource.ClientID,
                Payload     = new PyTuple (1) {[0] = new PySubStream (content)},
                OutOfBounds = outOfBounds ?? new PyDictionary ()
            }
        );
    }

    /// <summary>
    /// Sends an exception as answer to the given call
    /// </summary>
    /// <param name="answerTo"></param>
    /// <param name="packetType"></param>
    /// <param name="content"></param>
    public void SendException (CallInformation answerTo, PyPacket.PacketType packetType, PyDataType content)
    {
        // ensure destination has clientID in it
        PyAddressClient source = answerTo.Source as PyAddressClient;

        source.ClientID = answerTo.Session.UserID;

        // build a new packet with the correct information
        PyPacket result = new PyPacket (PyPacket.PacketType.ERRORRESPONSE)
        {
            // switch source and dest
            Source      = answerTo.Destination,
            Destination = source,
            UserID      = source.ClientID,
            Payload = new PyTuple (3)
            {
                [0] = (int) packetType,
                [1] = (int) MachoErrorType.WrappedException,
                [2] = new PyTuple (1) {[0] = new PySubStream (content)}
            }
        };

        MachoNet.QueueOutputPacket (null, result);
    }

    /// <summary>
    /// Sends an exception as answer to the given call
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="packetType"></param>
    /// <param name="content"></param>
    /// <param name="source"></param>
    public void SendException (PyAddress source, PyAddress destination, PyPacket.PacketType packetType, PyDataType content)
    {
        int userID = 0;

        if (destination is PyAddressClient client)
            userID = client.ClientID;

        PyPacket result = new PyPacket (PyPacket.PacketType.ERRORRESPONSE)
        {
            Source      = source,
            Destination = destination,
            UserID      = userID,
            Payload = new PyTuple (3)
            {
                [0] = (int) packetType,
                [1] = (int) MachoErrorType.WrappedException,
                [2] = new PyTuple (1) {[0] = new PySubStream (content)}
            }
        };

        MachoNet.QueueOutputPacket (null, result);
    }
}