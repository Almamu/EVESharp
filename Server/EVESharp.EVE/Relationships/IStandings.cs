using EVESharp.EVE.Data.Standings;

namespace EVESharp.EVE.Relationships;

public interface IStandings
{
    /// <summary>
    /// Updates the standings from <see cref="fromID"/> to <see cref="toID"/> to the new value
    /// </summary>
    /// <param name="ev">The type of event that triggered the standing update</param>
    /// <param name="fromID">The entity that generated the standing change</param>
    /// <param name="toID">The entity that receives the standing change</param>
    /// <param name="newStanding">The new standing value</param>
    /// <param name="reason">Description of why the change happened</param>
    public void SetStanding (EventType ev, int fromID, int toID, double newStanding, string reason);
}