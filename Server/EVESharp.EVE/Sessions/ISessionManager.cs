using System.Collections.Generic;

namespace EVESharp.EVE.Sessions;

public interface ISessionManager
{
    void InitializeSession (Session session);

    /// <summary>
    /// Updates sessions based on the idType and id as criteria
    /// </summary>
    /// <param name="idType"></param>
    /// <param name="id"></param>
    /// <param name="newValues">The new values for the session</param>
    void PerformSessionUpdate (string idType, int id, Session newValues);

    new void FreeSession (Session session);

    /// <summary>
    /// Searches for the requested session based on the idType and the value for that id
    /// </summary>
    /// <param name="idType">The value to filter by</param>
    /// <param name="id">The id's value</param>
    /// <returns>The list of sessions found (if any)</returns>
    List<Session> FindSession(string idType, int id);
}