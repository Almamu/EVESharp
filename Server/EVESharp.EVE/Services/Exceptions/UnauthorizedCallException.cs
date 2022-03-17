using System;

namespace EVESharp.EVE.Services.Exceptions;

public class UnauthorizedCallException<T> : Exception
{
    public string Call { get; init; }
    public T Service { get; init; }
    public ulong Roles { get; init; }

    public UnauthorizedCallException(T service, string call, ulong roles)
    {
        this.Service = service;
        this.Call = call;
    }
}