using System;
using System.Text.RegularExpressions;
using EVESharp.EVE.Data.Account;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Network;
using EVESharp.EVE.Network.Messages;
using EVESharp.EVE.Network.Transports;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Services;
using EVESharp.EVE.Sessions;
using EVESharp.EVE.Types.Network;
using EVESharp.Node.Server.Shared.Helpers;
using EVESharp.Node.Services;
using EVESharp.Types;
using EVESharp.Types.Collections;
using Serilog;

namespace EVESharp.Node.Server.Shared.Handlers;

public class LocalCallHandler
{
    private int                  ErrorID              { get; set; }
    public  IMachoNet            MachoNet             { get; }
    public  ServiceManager       ServiceManager       { get; }
    public  BoundServiceManager  BoundServiceManager  { get; }
    public  IRemoteServiceManager RemoteServiceManager { get; }
    public  ILogger              Log                  { get; }
    public  PacketCallHelper     PacketCallHelper     { get; }

    public LocalCallHandler
    (
        IMachoNet machoNet, ILogger logger, ServiceManager serviceManager, BoundServiceManager boundServiceManager, IRemoteServiceManager remoteServiceManager,
        PacketCallHelper packetCallHelper
    )
    {
        MachoNet             = machoNet;
        ServiceManager       = serviceManager;
        BoundServiceManager  = boundServiceManager;
        RemoteServiceManager = remoteServiceManager;
        Log                  = logger;
        PacketCallHelper     = packetCallHelper;
    }

    private PyDataType HandleNormalCallReq (CallInformation call, PyTuple information, string service, string method)
    {
        MachoNet.Log.Verbose ($"Calling {service}::{method}");

        return ServiceManager.ServiceCall (service, method, call);
    }

    private PyDataType HandleBoundCallReq (CallInformation call, PyTuple information, string method)
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
                callResult = this.HandleBoundCallReq (callInformation, info, call);
            else
                callResult = this.HandleNormalCallReq (callInformation, info, service, call);

            PacketCallHelper.SendCallResult (callInformation, callResult, callInformation.ResultOutOfBounds);
        }
        catch (PyException e)
        {
            PacketCallHelper.SendException (callInformation, packet.Type, e);
        }
        catch (ProvisionalResponse provisional)
        {
            PacketCallHelper.SendProvisionalResponse (callInformation, provisional);
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
                PacketCallHelper.SendException (
                    callInformation, packet.Type,
                    new CustomError (
                        $"An internal server error occurred.<br><b>Reference</b>: {errorID}<br><b>Message</b>: {ex.Message}<br><b>Stack trace</b>:<br>{ex.StackTrace.Replace ("\n", "<br>")}"
                    )
                );
            else
                PacketCallHelper.SendException (
                    callInformation, packet.Type, new CustomError ($"An internal server error occurred. <b>Reference</b>: {errorID}")
                );
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

        Session playerSession = null;

        if (machoMessage.Transport is MachoClientTransport clientTransport)
            playerSession = clientTransport.Session;

        if (machoMessage.Packet.OutOfBounds?.TryGetValue ("Session", out PyDictionary session) == true)
            playerSession = Session.FromPyDictionary (session);

        RemoteServiceManager.ReceivedRemoteCallAnswer (dest.CallID, subStream.Stream, playerSession);
    }
}