using EVESharp.EVE.Client.Exceptions;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Sessions;
using Org.BouncyCastle.Crypto.Tls;

namespace EVESharp.Node.Sessions;

public static class SessionExtensions
{
    /// <summary>
    /// Checks session data to ensure a character is selected and returns it's characterID
    /// </summary>
    /// <returns>CharacterID for the client</returns>
    public static int EnsureCharacterIsSelected (this Session session)
    {
        if (session.ContainsKey (Session.CHAR_ID) == false)
            throw new CustomError ("NoCharacterSelected");

        return session.CharacterID;
    }

    /// <summary>
    /// Checks session data to ensure the character is in a station
    /// </summary>
    /// <returns>The StationID where the character is at</returns>
    /// <exception cref="CanOnlyDoInStations"></exception>
    public static int EnsureCharacterIsInStation (this Session session)
    {
        if (session.ContainsKey (Session.STATION_ID) == false)
            throw new CanOnlyDoInStations ();

        return session.StationID;
    }
}