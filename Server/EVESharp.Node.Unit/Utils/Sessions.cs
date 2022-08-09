using EVESharp.EVE.Sessions;

namespace EVESharp.Node.Unit.Utils;

public static class Sessions
{
    public const int CHARACTERID = 1000;
    
    public static Session CreateSession ()
    {
        Session session = new Session ();

        session.CharacterID = CHARACTERID; 
        
        return session;
    }
}