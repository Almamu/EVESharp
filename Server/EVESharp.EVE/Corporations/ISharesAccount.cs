using System;

namespace EVESharp.EVE.Corporations;

public interface ISharesAccount : IDisposable
{
    public int OwnerID { get; set; }

    /// <summary>
    /// Obtains the current shares the account has for the given <see cref="corporationID"/>
    /// </summary>
    /// <param name="corporationID"></param>
    /// <returns></returns>
    public uint GetSharesForCorporation (int corporationID);
    
    /// <summary>
    /// Updates the shares the account has for the given <see cref="corporationID"/>
    /// </summary>
    /// <param name="corporationID"></param>
    /// <param name="newSharesCount"></param>
    public void UpdateSharesForCorporation (int corporationID, uint newSharesCount);
}