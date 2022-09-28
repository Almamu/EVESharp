using System;

namespace EVESharp.EVE.Network.Services.Exceptions;

public class MissingServiceException<T> : Exception
{
    public string Call { get; init; }
    public T Service { get; init; }

    public MissingServiceException(T service, string call)
    {
        this.Service = service;
        this.Call    = call;
    }
}