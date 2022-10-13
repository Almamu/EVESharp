using EVESharp.Database.Old;
using EVESharp.Database.Standings;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Notifications.Character;
using EVESharp.EVE.Relationships;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Node.Relationships;

public class Standings : IStandings
{
    private StandingDB          DB            { get; }
    private INotificationSender Notifications { get; }
    
    public Standings (StandingDB db, INotificationSender notifications)
    {
        this.DB            = db;
        this.Notifications = notifications;
    }
    
    public void SetStanding (EventType ev, int fromID, int toID, double newStanding, string reason)
    {
        DB.CreateStandingTransaction ((int) ev, fromID, toID, newStanding, reason);
        DB.SetPlayerStanding (fromID, toID, newStanding);

        // send the same notification to both players
        Notifications.NotifyOwners (
            new PyList <PyInteger> (2)
            {
                [0] = fromID,
                [1] = toID
            },
            new OnStandingSet (fromID, toID, newStanding)
        );
    }
}