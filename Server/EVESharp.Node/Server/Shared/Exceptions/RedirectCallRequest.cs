using System;

namespace EVESharp.Node.Server.Shared.Exceptions;

public class RedirectCallRequest : Exception
{
    public int NodeID { get; }

    public RedirectCallRequest (int nodeID)
    {
        this.NodeID = nodeID;
    }
}