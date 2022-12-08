using EVESharp.EVE.Sessions;

namespace EVESharp.Node.Unit.Utils;

public static class Sessions
{
    public const int CHARACTERID = 1000;
    
    public static Session CreateSession ()
    {
        return new Session
        {
            CharacterID = CHARACTERID
        };
    }
}