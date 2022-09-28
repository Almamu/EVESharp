using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using EVESharp.EVE.Network;
using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Network.Services.Exceptions;
using EVESharp.EVE.Network.Transports;
using EVESharp.EVE.Sessions;
using EVESharp.Types;
using Serilog;

namespace EVESharp.Node.Services;

/// <summary>
/// Special service manager that handles Bound objects from the client
///
/// These bound objects are usually stateful services that keep information about the player,
/// location, items, etc, and is used as a way of managing the resources
/// </summary>
public class BoundServiceManager : IBoundServiceManager
{
    private int                            mNextBoundID = 1;
    public  IMachoNet                      MachoNet       { get; }
    public  Dictionary <int, BoundService> BoundServices  { get; } = new Dictionary <int, BoundService> ();
    private ILogger                        Log            { get; }
    private ISessionManager                SessionManager { get; }

    public BoundServiceManager (IMachoNet machoNet, ILogger logger, ISessionManager sessionManager)
    {
        MachoNet       = machoNet;
        Log            = logger;
        SessionManager = sessionManager;
        // register on transport closed so proxies and single-instance servers cleanup properly
        MachoNet.TransportManager.TransportRemoved += this.OnTransportRemoved;
        // register session freeing so proxies and single-instance server cleanup properly
        SessionManager.OnSessionFreed += this.OnClientDisconnected;
    }

    /// <summary>
    /// Searches for the given bound service ID and handles the requested call
    /// </summary>
    /// <param name="boundID">The bound service ID</param>
    /// <param name="method">The method to call</param>
    /// <param name="call">Information about the call to perform</param>
    /// <returns>The return from the service call</returns>
    /// <exception cref="MissingServiceException{T}"></exception>
    /// <exception cref="UnauthorizedCallException{int}"></exception>
    public PyDataType ServiceCall (int boundID, string method, ServiceCall call)
    {
        if (BoundServices.TryGetValue (boundID, out BoundService service) == false)
            throw new MissingServiceException <int> (boundID, method);

        if (service.IsClientAllowedToCall (call.Session) == false)
            throw new UnauthorizedCallException <int> (boundID, method, call.Session.Role);

        Log.Verbose ($"Calling {service.Name}::{method} on bound service {boundID}");

        return service.ExecuteCall (method, call);
    }

    /// <summary>
    /// Registers the given bound service into this service manager
    /// </summary>
    /// <param name="service">The bound service to register</param>
    /// <returns>The boundID of this service</returns>
    public int BindService (BoundService service)
    {
        lock (BoundServices)
        {
            int boundID = this.mNextBoundID++;

            // add the service to the bound services map
            BoundServices [boundID] = service;

            return boundID;
        }
    }

    /// <summary>
    /// Removes the bound service and unregisters it from the manager
    /// </summary>
    /// <param name="service">The service to unbind</param>
    public void UnbindService (BoundService service)
    {
        Log.Debug ($"Unbinding service {service.BoundID}");

        // remove the service from the bound list
        lock (BoundServices)
        {
            BoundServices.Remove (service.BoundID);
        }
    }

    /// <param name="boundID">The boundID to generate the string for</param>
    /// <returns>A string representation of the given boundID</returns>
    public string BuildBoundServiceString (int boundID)
    {
        return $"N={MachoNet.NodeID}:{boundID}";
    }

    public static void ParseBoundServiceString (string guid, out int nodeID, out int boundID)
    {
        // parse the bound string to get back proper node and bound ids
        Match regexMatch = Regex.Match (guid, "N=([0-9]+):([0-9]+)");

        if (regexMatch.Groups.Count != 3)
            throw new Exception ($"Cannot find nodeID and boundID in the boundString {guid}");

        nodeID  = int.Parse (regexMatch.Groups [1].Value);
        boundID = int.Parse (regexMatch.Groups [2].Value);
    }

    public void ClientHasReleasedThisObject (int boundID, Session session)
    {
        if (BoundServices.TryGetValue (boundID, out BoundService svc) == false)
            return;

        svc.ClientHasReleasedThisObject (session);
    }

    public void OnClientDisconnected (Session session)
    {
        foreach ((int _, BoundService service) in BoundServices)
        {
            if (service.IsClientAllowedToCall (session) == false)
                continue;

            service.ClientHasReleasedThisObject (session);
        }
    }

    private void OnTransportRemoved (IMachoTransport transport)
    {
        if (transport is not MachoClientTransport clientTransport)
            return;

        this.OnClientDisconnected (clientTransport.Session);
    }
}