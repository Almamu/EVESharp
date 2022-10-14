namespace EVESharp.EVE.Corporations;

public interface IShares
{
    /// <summary>
    /// Provides access to a specific shares account
    /// (glorified database locking to prevent multiple changes at the same time)
    /// </summary>
    /// <param name="ownerID">The shares account to get access to</param>
    /// <returns></returns>
    public ISharesAccount AcquireSharesAccount (int ownerID);
}