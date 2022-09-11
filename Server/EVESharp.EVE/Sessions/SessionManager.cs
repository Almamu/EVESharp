using System.Collections.Generic;
using System.IO;
using System.Linq;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Sessions;

/// <summary>
/// A simple session manager to keep track of all the sessions registered
/// </summary>
public abstract class SessionManager : ISessionManager
{
    /// <summary>
    /// The real storage of sessions happens here
    /// </summary>
    private readonly Dictionary<int, Session> mSessions = new Dictionary<int,Session>();

    /// <summary>
    /// Registers a new session in the list
    /// </summary>
    /// <param name="source">The session to register</param>
    protected void RegisterSession(Session source)
    {
        this.mSessions.Add(source.UserID, source);
    }

    public abstract void InitializeSession (Session   session);
    public abstract void PerformSessionUpdate (string idType, int id, Session newValues);
    void ISessionManager.FreeSession (Session         session)
    {
        this.FreeSession (session);
    }

    /// <summary>
    /// Frees the given session from the list
    /// </summary>
    /// <param name="source">The session to free</param>
    protected void FreeSession(Session source)
    {
        this.mSessions.Remove(source.UserID);
    }

    /// <summary>
    /// Searches for the requested session based on the idType and the value for that id
    /// </summary>
    /// <param name="idType">The value to filter by</param>
    /// <param name="id">The id's value</param>
    /// <returns>The list of sessions found (if any)</returns>
    public List<Session> FindSession(string idType, int id)
    {
        return this.mSessions
            .Where(x =>
            {
                if (x.Value.TryGetValue(idType, out PyDataType value) == false)
                    return false;

                return value == id;
            })
            .Select(x => x.Value)
            .ToList();
    }
    
    /// <summary>
    /// Updates attributes and returns a delta of the differences
    /// </summary>
    /// <param name="current">The session to update</param>
    /// <param name="values">The new values to set</param>
    /// <returns>The delta of the session</returns>
    /// <exception cref="InvalidDataException">If the session cannot be found</exception>
    protected static SessionChange UpdateAttributes(Session current, Session values)
    {
        SessionChange changes = new SessionChange();
        
        // get the new values and compare which ones have changed
        foreach ((PyString key, PyDataType newValue) in values)
        {
            // check for non-existent values first
            if (current.TryGetValue(key, out PyDataType value) == false)
            {
                // add the change to the list
                changes.AddChange(key, null, newValue);
                // update the session value
                current[key] = newValue;
                continue;
            }

            // value exists, compare them
            if (value == newValue)
                continue;

            // add the change to the list
            changes.AddChange(key, value, newValue);
            // update the session value
            current[key] = newValue;
        }
        
        // changes should have all the updated values, return the difference so notifications can be sent
        return changes;
    }
}