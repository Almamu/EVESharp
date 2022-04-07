using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Exceptions;

namespace EVESharp.Node.Sessions;

public static class SessionExtensions
{
    /// <summary>
    /// Checks session data to ensure a character is selected and returns it's characterID
    /// </summary>
    /// <returns>CharacterID for the client</returns>
    public static int EnsureCharacterIsSelected (this Session session)
    {
        int? characterID = session.CharacterID;

        if (characterID is null)
            throw new CustomError ("NoCharacterSelected");

        return (int) characterID;
    }

    /// <summary>
    /// Checks session data to ensure the character is in a station
    /// </summary>
    /// <returns>The StationID where the character is at</returns>
    /// <exception cref="CanOnlyDoInStations"></exception>
    public static int EnsureCharacterIsInStation (this Session session)
    {
        int? stationID = session.StationID;

        if (stationID is null)
            throw new CanOnlyDoInStations ();

        return (int) stationID;
    }
}