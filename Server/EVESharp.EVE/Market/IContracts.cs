using EVESharp.Database.Market;

namespace EVESharp.EVE.Market;

public interface IContracts
{
    /// <summary>
    /// Obtains an exclusive lock on the given contractID and fetches the relevant information
    /// </summary>
    /// <param name="contractID"></param>
    /// <returns></returns>
    IContract AcquireContract (int contractID);

    /// <summary>
    /// Creates a new contract with the given information and obtains it's lock to work on it
    /// </summary>
    /// <param name="characterID"></param>
    /// <param name="corporationID"></param>
    /// <param name="allianceID"></param>
    /// <param name="type"></param>
    /// <param name="availability"></param>
    /// <param name="assigneeID"></param>
    /// <param name="expireTime"></param>
    /// <param name="duration"></param>
    /// <param name="startStationID"></param>
    /// <param name="endStationID"></param>
    /// <param name="price"></param>
    /// <param name="reward"></param>
    /// <param name="collateral"></param>
    /// <param name="title"></param>
    /// <param name="description"></param>
    /// <param name="issuerWalletID"></param>
    /// <returns></returns>
    IContract CreateContract (
        int    characterID, int    corporationID, int?   allianceID, ContractTypes type,         int    availability,
        int    assigneeID,  int    expireTime,    int    duration,   int    startStationID, int?          endStationID, double price,
        double reward,      double collateral,    string title,      string description,    int           issuerWalletID
    );
}