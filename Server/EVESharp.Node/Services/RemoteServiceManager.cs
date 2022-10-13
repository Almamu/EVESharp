using System;
using System.Collections.Generic;
using EVESharp.Database;
using EVESharp.Database.Extensions;
using EVESharp.EVE;
using EVESharp.EVE.Data.Account;
using EVESharp.EVE.Network;
using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Sessions;
using EVESharp.EVE.Types.Network;
using EVESharp.Types;
using EVESharp.Types.Collections;
using Serilog;

namespace EVESharp.Node.Services;

public class RemoteServiceManager : IRemoteServiceManager
{
    private readonly Dictionary <int, RemoteCall> mCallCallbacks = new Dictionary <int, RemoteCall> ();
    private          int                          mNextCallID;

    private ITimers             Timers   { get; }
    private IMachoNet           MachoNet { get; }
    private IDatabase Database { get; }

    public RemoteServiceManager (IMachoNet machoNet, ITimers timers, IDatabase database)
    {
        Timers   = timers;
        MachoNet = machoNet;
        Database = database;
    }

    /// <summary>
    /// Callback fired by the <seealso cref="Timers"/> when a call timeout has been reached
    /// </summary>
    /// <param name="callID">The callID that expired</param>
    protected void CallTimeoutExpired (int callID)
    {
        lock (this.mCallCallbacks)
        {
            Log.Warning ($"Timeout for call {callID} expired before getting an answer.");

            // get call id and call the timeout callback
            RemoteCall call = this.mCallCallbacks [callID];

            // call the callback if available
            call.TimeoutCallback?.Invoke (call);

            // finally remove from the list
            this.mCallCallbacks.Remove (callID);
        }
    }

    /// <summary>
    /// Tells the RemoteServiceManager that a call was completed successfully and invokes the success callback
    /// so the server can continue processing further
    /// </summary>
    /// <param name="callID">The callID that completed</param>
    /// <param name="result">The result of the call</param>
    /// <param name="answerSession">The session of the client that answered the call</param>
    public void ReceivedRemoteCallAnswer (int callID, PyDataType result, Session answerSession)
    {
        lock (this.mCallCallbacks)
        {
            if (this.mCallCallbacks.TryGetValue (callID, out RemoteCall call) == false)
            {
                Log.Warning ($"Received an answer for call {callID} after the timeout expired, ignoring answer...");

                return;
            }

            // ensure the session data is there now
            call.Session ??= answerSession;

            // stop the timer
            call.Timer?.Dispose ();

            // invoke the handler
            call.Callback?.Invoke (call, result);

            // remove the call from the list
            this.mCallCallbacks.Remove (callID);
        }
    }

    /// <summary>
    /// Reserves a slot in the call list and prepares timeout timers in case the call wait time expires 
    /// </summary>
    /// <param name="entry">The RemoteCall entry to associate with this call</param>
    /// <param name="timeoutSeconds">The amount of seconds to wait until timing out</param>
    /// <returns>The callID to be notified to the client</returns>
    private int ExpectRemoteServiceResult (RemoteCall entry, int timeoutSeconds = 0)
    {
        lock (this.mCallCallbacks)
        {
            // get the new callID
            int callID = ++this.mNextCallID;

            // add the callback to the list
            this.mCallCallbacks [callID] = entry;

            // create the timeout timer if needed
            if (timeoutSeconds > 0)
                entry.Timer = Timers.EnqueueTimer (
                    DateTime.UtcNow.AddSeconds (timeoutSeconds),
                    this.CallTimeoutExpired,
                    callID
                );

            return callID;
        }
    }

    /// <summary>
    /// Reserves a slot in the call list and prepares timeout timers in case the call wait time expires 
    /// </summary>
    /// <param name="callback">The function to call when the answer is received</param>
    /// <param name="extraInfo">Any extra information to store for later usage</param>
    /// <param name="timeoutCallback">The function to call if the call timeout expires</param>
    /// <param name="timeoutSeconds">The amount of seconds to wait until timing out</param>
    /// <returns>The callID to be notified to the client</returns>
    protected int ExpectRemoteServiceResult
        (Action <RemoteCall, PyDataType> callback, object extraInfo = null, Action <RemoteCall> timeoutCallback = null, int timeoutSeconds = 0)
    {
        RemoteCall entry = new RemoteCall
        {
            Callback        = callback,
            ExtraInfo       = extraInfo,
            TimeoutCallback = timeoutCallback
        };

        return this.ExpectRemoteServiceResult (entry, timeoutSeconds);
    }

    private PyPacket BuildCallRequestPacket (string service, string method, int clientID, PyTuple args, PyDictionary namedPayload, int callID)
    {
        return new PyPacket (PyPacket.PacketType.CALL_REQ)
        {
            Destination = new PyAddressClient (clientID, callID, service),
            Source      = new PyAddressNode (MachoNet.NodeID, callID),
            OutOfBounds = new PyDictionary {["role"] = (int) Roles.ROLE_SERVICE | (int) Roles.ROLE_REMOTESERVICE},
            Payload = new PyTuple (2)
            {
                [0] = new PyTuple (2)
                {
                    [0] = 0,
                    [1] = new PySubStream (
                        new PyTuple (4)
                        {
                            [0] = 1,
                            [1] = method,
                            [2] = args,
                            [3] = namedPayload
                        }
                    )
                },
                [1] = null
            }
        };
    }

    public void SendServiceCall
    (
        int                             characterID,     string              service,                string call, PyTuple args, PyDictionary namedPayload,
        Action <RemoteCall, PyDataType> callback = null, Action <RemoteCall> timeoutCallback = null, object extraInfo = null, int timeoutSeconds = 0
    )
    {
        // resolve the characterID to a clientID
        int clientID = Database.CluResolveCharacter (characterID);
        // queue the call in the service manager and get the callID
        int callID = this.ExpectRemoteServiceResult (callback, extraInfo, timeoutCallback, timeoutSeconds);

        // generate the actual packet and send it back
        PyPacket packet = BuildCallRequestPacket (service, call, clientID, args, namedPayload, callID);

        // everything is ready, send the packet to the client
        MachoNet.QueueOutputPacket (packet);
    }

    public void SendServiceCall
    (
        Session                         session,         string              service,                string call, PyTuple args, PyDictionary namedPayload,
        Action <RemoteCall, PyDataType> callback = null, Action <RemoteCall> timeoutCallback = null, object extraInfo = null, int timeoutSeconds = 0
    )
    {
        this.SendServiceCall (
            session.CharacterID, service, call, args, namedPayload, callback, timeoutCallback, extraInfo,
            timeoutSeconds
        );
    }
}