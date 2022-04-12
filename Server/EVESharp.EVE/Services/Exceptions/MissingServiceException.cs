using System;

namespace EVESharp.EVE.Services.Exceptions;

public class MissingServiceException<T> : Exception
{
    public string Call { get; init; }
    public T Service { get; init; }

    public MissingServiceException(T service, string call)
    {
        Service = service;
        Call = call;
    }
}