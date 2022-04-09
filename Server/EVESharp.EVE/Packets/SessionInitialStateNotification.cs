using System.IO;
using System.Runtime.CompilerServices;
using EVESharp.EVE.Sessions;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.EVE.Packets;

public class SessionInitialStateNotification
{
    public Session Session { get; init; }

    public static implicit operator PyTuple(SessionInitialStateNotification notification)
    {
        return new PyTuple(1)
        {
            [0] = notification.Session
        };
    }

    public static implicit operator SessionInitialStateNotification(PyTuple origin)
    {
        if (origin.Count != 1)
            throw new InvalidDataException("Expected a tuple with one element");

        if (origin[0] is PyDictionary == false)
            throw new InvalidDataException("The first element must be a dictionary");

        return new SessionInitialStateNotification()
        {
            Session = Session.FromPyDictionary(origin[0] as PyDictionary)
        };
    }
}