using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using EVESharp.EVE.Sessions;

namespace EVESharp.EVE.Network.Services;

public interface IBoundServiceManager : IServiceManager <int>
{
    public IMachoNet                      MachoNet      { get; }
    public Dictionary <int, BoundService> BoundServices { get; }
    
    /// <summary>
    /// Registers the given bound service into this service manager
    /// </summary>
    /// <param name="service">The bound service to register</param>
    /// <returns>The boundID of this service</returns>
    public int BindService (BoundService service);

    /// <summary>
    /// Removes the bound service and unregisters it from the manager
    /// </summary>
    /// <param name="service">The service to unbind</param>
    public void UnbindService (BoundService service);

    /// <param name="boundID">The boundID to generate the string for</param>
    /// <returns>A string representation of the given boundID</returns>
    public string BuildBoundServiceString (int boundID);

    public static void ParseBoundServiceString (string guid, out int nodeID, out int boundID)
    {
        // parse the bound string to get back proper node and bound ids
        Match regexMatch = Regex.Match (guid, "N=([0-9]+):([0-9]+)");

        if (regexMatch.Groups.Count != 3)
            throw new Exception ($"Cannot find nodeID and boundID in the boundString {guid}");

        nodeID  = int.Parse (regexMatch.Groups [1].Value);
        boundID = int.Parse (regexMatch.Groups [2].Value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="boundID"></param>
    /// <param name="session"></param>
    public void ClientHasReleasedThisObject (int boundID, Session session);
}