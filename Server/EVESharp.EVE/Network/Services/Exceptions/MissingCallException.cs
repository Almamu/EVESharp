using System;

namespace EVESharp.EVE.Network.Services.Exceptions;

public class MissingCallException : Exception
{
    public string Call { get; init; }
    public string Service { get; init; }

    public MissingCallException(string service, string call)
    {
        this.Service = service;
        this.Call    = call;
    }
}