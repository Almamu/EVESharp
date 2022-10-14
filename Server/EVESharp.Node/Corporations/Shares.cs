using EVESharp.Database;
using EVESharp.EVE.Corporations;

namespace EVESharp.Node.Corporations;

public class Shares : IShares
{
    private IDatabase Database { get; }

    public Shares (IDatabase database)
    {
        Database = database;
    }
    
    public ISharesAccount AcquireSharesAccount (int ownerID)
    {
        return new SharesAccount (ownerID, this.Database);
    }
}