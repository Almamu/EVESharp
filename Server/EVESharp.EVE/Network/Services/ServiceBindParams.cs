using System;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Network.Services;

public class ServiceBindParams
{
    public int ObjectID   { get; init; }
    public int ExtraValue { get; init; }

    public static implicit operator ServiceBindParams (PyDataType from)
    {
        // bind parameters for services can be different
        // eve doesn't have a specific format for them
        // as there's different things required for each service, but analyzing the client
        // looks like most of them are either an integer, or a tuple with one or two integers
        // TODO: HANDLE EXTRA CASE WHERE A TUPLE CAN HAVE A STRING AS FIRST ELEMENT
        if (from is PyInteger objectID)
            return new ServiceBindParams {ObjectID = objectID};

        if (from is not PyTuple tuple)
            throw new Exception ("Unknown bind params for service");

        if (tuple.Count == 0 || tuple [0] is not PyInteger objectID2)
            throw new Exception ("First bind param must be an integer");

        if (tuple.Count == 1)
            return new ServiceBindParams {ObjectID = objectID2};

        if (tuple [1] is not PyInteger extraValue)
            throw new Exception ("Second bind param must be an integer");

        return new ServiceBindParams
        {
            ObjectID   = objectID2,
            ExtraValue = extraValue
        };
    }
}