using System;

namespace EVESharp.EVE.Services.Exceptions;

public class MissingCallException : Exception
{
    public string Call { get; init; }
    public string Service { get; init; }

    public MissingCallException(string service, string call)
    {
        Service = service;
        Call = call;
    }
}