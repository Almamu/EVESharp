using System;
using System.Text.RegularExpressions;
using EVESharp.EVE;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Network;
using EVESharp.Node.Server.Shared.Messages;
using EVESharp.Node.Services;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;
using Serilog;

namespace EVESharp.Node.Server.Shared.Handlers;

public class LocalCallHandler
{
    private int                 ErrorID             { get; set; }
    public  IMachoNet           MachoNet            { get; }
    public  MessageProcessor    MessageProcessor    { get; }
    public  ServiceManager      ServiceManager      { get; }
    public  BoundServiceManager BoundServiceManager { get; }
    public  ILogger             Log                 { get; }

    public LocalCallHandler (
        IMachoNet machoNet, MessageProcessor processor, ILogger logger, ServiceManager serviceManager, BoundServiceManager boundServiceManager
    )
    {
        MachoNet            = machoNet;
        MessageProcessor    = processor;
        ServiceManager      = serviceManager;
        BoundServiceManager = boundServiceManager;
        Log                 = logger;
    }

    private PyDataType HandleNormalCallReq (PyTuple information, string service, string method, CallInformation call)
    {
        MachoNet.Log.Verbose ($"Calling {service}::{method}");

        return ServiceManager.ServiceCall (service, method, call);
    }

    private PyDataType HandleBoundCallReq (PyTuple information, string method, CallInformation call)
    {
        if (information [0] is PyString == false)
            throw new Exception ("Expected bound call with bound string, but got something different");

        string boundString = information [0] as PyString;

        // parse the bound string to get back proper node and bound ids
        Match regexMatch = Regex.Match (boundString, "N=([0-9]+):([0-9]+)");

        if (regexMatch.Groups.Count != 3)
            throw new Exception ($"Cannot find nodeID and boundID in the boundString {boundString}");

        int nodeID  = int.Parse (regexMatch.Groups [1].Value);
        int boundID = int.Parse (regexMatch.Groups [2].Value);

        if (nodeID != MachoNet.NodeID)
            throw new Exception ("Got bound service call for a different node");

        return BoundServiceManager.ServiceCall (boundID, method, call);
    }

    public void HandleCallReq (MachoMessage machoMessage)
    {
        PyPacket packet = machoMessage.Packet;
        PyTuple  info   = ((packet.Payload [0] as PyTuple) [1] as PySubStream).Stream as PyTuple;

        string          call       = info [1] as PyString;
        PyTuple         args       = info [2] as PyTuple;
        PyDictionary    sub        = info [3] as PyDictionary;
        PyDataType      callResult = null;
        PyAddressClient source     = packet.Source as PyAddressClient;
        string          service    = null;

        if (packet.Destination is PyAddressAny any)
        {
            service = any.Service;
        }
        else if (packet.Destination is PyAddressNode node)
        {
            service = node.Service;

            if (node.NodeID != MachoNet.NodeID)
                throw new Exception ("Received a call request for a node that is not us on a single server instance. Hacking much?!");
        }

        Session session = machoMessage.Transport.Session;

        if (machoMessage.Packet.OutOfBounds is not null && machoMessage.Packet.OutOfBounds.TryGetValue ("Session", out PyDictionary dictionary))
            session = Session.FromPyDictionary (dictionary);

        CallInformation callInformation = new CallInformation
        {
            Transport           = machoMessage.Transport,
            CallID              = source.CallID,
            Payload             = args,
            NamedPayload        = sub,
            Source              = packet.Source,
            Destination         = packet.Destination,
            MachoNet            = MachoNet,
            Session             = session,
            ResultOutOfBounds   = new PyDictionary <PyString, PyDataType> (),
            ServiceManager      = ServiceManager,
            BoundServiceManager = BoundServiceManager
        };

        try
        {
            if (service is null)
                callResult = this.HandleBoundCallReq (info, call, callInformation);
            else
                callResult = this.HandleNormalCallReq (info, service, call, callInformation);

            this.SendCallResult (callInformation, callResult, callInformation.ResultOutOfBounds);
        }
        catch (PyException e)
        {
            this.SendException (callInformation, packet.Type, e);
        }
        catch (ProvisionalResponse provisional)
        {
            this.SendProvisionalResponse (callInformation, provisional);
        }
        catch (Exception ex)
        {
            int errorID = ++ErrorID;

            Log.Fatal (
                "Detected non-client exception on call to {service}::{call}, registered as error {errorID}. Extra information: \n{3}\n{4}",
                service, call, errorID, ex.Message, ex.StackTrace
            );

            // send client a proper notification about the error based on the roles
            if ((callInformation.Session.Role & (int) Roles.ROLE_PROGRAMMER) == (int) Roles.ROLE_PROGRAMMER)
                this.SendException (
                    callInformation, packet.Type,
                    new CustomError (
                        $"An internal server error occurred.<br><b>Reference</b>: {errorID}<br><b>Message</b>: {ex.Message}<br><b>Stack trace</b>:<br>{ex.StackTrace.Replace ("\n", "<br>")}"
                    )
                );
            else
                this.SendException (callInformation, packet.Type, new CustomError ($"An internal server error occurred. <b>Reference</b>: {errorID}"));
        }
    }

    public void HandleCallRsp (MachoMessage machoMessage)
    {
        if (machoMessage.Packet.Destination is not PyAddressNode dest)
            throw new Exception ("Received a call response not directed to a node");

        if (machoMessage.Packet.Payload.Count != 1)
            throw new Exception ("Received a call response without proper response data");

        PyDataType first = machoMessage.Packet.Payload [0];

        if (first is PySubStream == false)
            throw new Exception ("Received a call response without proper response data");

        PySubStream subStream = machoMessage.Packet.Payload [0] as PySubStream;

        ServiceManager.ReceivedRemoteCallAnswer (dest.CallID, subStream.Stream);
    }

    /// <summary>
    /// Sends a provisional response to the given call
    /// </summary>
    /// <param name="answerTo"></param>
    /// <param name="response"></param>
    private void SendProvisionalResponse (CallInformation answerTo, ProvisionalResponse response)
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
            Payload     = new PyTuple (0),
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
    private void SendCallResult (CallInformation answerTo, PyDataType content, PyDictionary outOfBounds)
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
                OutOfBounds = outOfBounds
            }
        );
    }

    /// <summary>
    /// Sends an exception as answer to the given call
    /// </summary>
    /// <param name="answerTo"></param>
    /// <param name="packetType"></param>
    /// <param name="content"></param>
    private void SendException (CallInformation answerTo, PyPacket.PacketType packetType, PyDataType content)
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
    private void SendException (PyAddress source, PyAddress destination, PyPacket.PacketType packetType, PyDataType content)
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