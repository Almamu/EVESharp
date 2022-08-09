using System;
using EVESharp.EVE.Sessions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Services;

public interface IRemoteServiceManager
{
    /// <summary>
    /// Tells the RemoteServiceManager that a call was completed successfully and invokes the success callback
    /// so the server can continue processing further
    /// </summary>
    /// <param name="callID">The callID that completed</param>
    /// <param name="result">The result of the call</param>
    /// <param name="answerSession">The session of the client that answered the call</param>
    void ReceivedRemoteCallAnswer (int callID, PyDataType result, Session answerSession);

    void SendServiceCall
    (
        int                             characterID,     string              service,                string call, PyTuple args, PyDictionary namedPayload,
        Action <RemoteCall, PyDataType> callback = null, Action <RemoteCall> timeoutCallback = null, object extraInfo = null, int timeoutSeconds = 0
    );

    void SendServiceCall
    (
        Session                         session,         string              service,                string call, PyTuple args, PyDictionary namedPayload,
        Action <RemoteCall, PyDataType> callback = null, Action <RemoteCall> timeoutCallback = null, object extraInfo = null, int timeoutSeconds = 0
    );
}