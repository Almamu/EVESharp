using EVESharp.EVE.Data.Corporation;
using EVESharp.EVE.Data.Market;
using EVESharp.EVE.Sessions;

namespace EVESharp.EVE.Permissions;

public static class Market
{
    public static class Wallet
    {
        /// <summary>
        /// Checks if the given session is allowed to take from the given account
        /// </summary>
        /// <param name="session">The session to check permissions for</param>
        /// <param name="accountKey">The wallet key</param>
        /// <param name="ownerID">The owner of the wallet</param>
        /// <returns></returns>
        public static bool IsTakeAllowed (Session session, int accountKey, int ownerID)
        {
            if (ownerID == session.CharacterID)
                return true;

            if (ownerID == session.CorporationID)
            {
                // check for permissions
                // check if the character has any accounting roles and set the correct accountKey based on the data
                if (CorporationRole.AccountCanTake1.Is (session.CorporationRole) && accountKey == WalletKeys.MAIN)
                    return true;
                if (CorporationRole.AccountCanTake2.Is (session.CorporationRole) && accountKey == WalletKeys.SECOND)
                    return true;
                if (CorporationRole.AccountCanTake3.Is (session.CorporationRole) && accountKey == WalletKeys.THIRD)
                    return true;
                if (CorporationRole.AccountCanTake4.Is (session.CorporationRole) && accountKey == WalletKeys.FOURTH)
                    return true;
                if (CorporationRole.AccountCanTake5.Is (session.CorporationRole) && accountKey == WalletKeys.FIFTH)
                    return true;
                if (CorporationRole.AccountCanTake6.Is (session.CorporationRole) && accountKey == WalletKeys.SIXTH)
                    return true;
                if (CorporationRole.AccountCanTake7.Is (session.CorporationRole) && accountKey == WalletKeys.SEVENTH)
                    return true;
                if (CorporationRole.Accountant.Is (session.CorporationRole))
                    return true;
            }

            return false;
        }
        
        /// <summary>
        /// Checks if the given session is allowed to read the given account
        /// </summary>
        /// <param name="session">The session to check permissions for</param>
        /// <param name="accountKey">The wallet key</param>
        /// <param name="ownerID">The owner of the wallet</param>
        /// <returns></returns>
        public static bool IsAccessAllowed (Session session, int accountKey, int ownerID)
        {
            if (ownerID == session.CharacterID)
                return true;

            if (ownerID == session.CorporationID)
            {
                // check for permissions
                // check if the character has any accounting roles and set the correct accountKey based on the data
                if (CorporationRole.AccountCanQuery1.Is (session.CorporationRole) && accountKey == WalletKeys.MAIN)
                    return true;
                if (CorporationRole.AccountCanQuery2.Is (session.CorporationRole) && accountKey == WalletKeys.SECOND)
                    return true;
                if (CorporationRole.AccountCanQuery3.Is (session.CorporationRole) && accountKey == WalletKeys.THIRD)
                    return true;
                if (CorporationRole.AccountCanQuery4.Is (session.CorporationRole) && accountKey == WalletKeys.FOURTH)
                    return true;
                if (CorporationRole.AccountCanQuery5.Is (session.CorporationRole) && accountKey == WalletKeys.FIFTH)
                    return true;
                if (CorporationRole.AccountCanQuery6.Is (session.CorporationRole) && accountKey == WalletKeys.SIXTH)
                    return true;
                if (CorporationRole.AccountCanQuery7.Is (session.CorporationRole) && accountKey == WalletKeys.SEVENTH)
                    return true;

                // last chance, accountant role
                if (CorporationRole.Accountant.Is (session.CorporationRole))
                    return true;
                if (CorporationRole.JuniorAccountant.Is (session.CorporationRole))
                    return true;
            }

            return false;
        }
    }
}