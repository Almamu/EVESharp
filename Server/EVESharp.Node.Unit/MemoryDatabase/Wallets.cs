using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EVESharp.Node.Unit.MemoryDatabase;

public class Wallets
{
    /// <summary>
    /// Cleanup after tests using this memory database are done
    /// </summary>
    public static void Cleanup ()
    {
        Data.Clear ();
    }
    
    public static Dictionary <(int walletKey, int ownerID), double> Data = new Dictionary <(int walletKey, int ownerID), double> ();
}