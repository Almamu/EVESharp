using System;
using System.Collections.Generic;
using EVESharp.Common.Database;

namespace EVESharp.Database;

public static class AccountDB
{
    public static long CluResolveClientAddress (this DatabaseConnection Database, int clientID)
    {
        return Database.Scalar <long> (
            "CluResolveClientAddress",
            new Dictionary <string, object> {{"_clientID", clientID}}
        );
    }

    public static void CluRegisterClientAddress (this DatabaseConnection Database, int clientID, long proxyNodeID)
    {
        
        Database.Procedure (
            "CluRegisterClientAddress",
            new Dictionary <string, object>
            {
                {"_clientID", clientID},
                {"_proxyNodeID", proxyNodeID}
            }
        );
    }

    public static void CluResetClientAddresses (this DatabaseConnection Database)
    {
        Database.Procedure ("CluResetClientAddresses");
    }

    public static bool ActExists (this DatabaseConnection Database, string username)
    {
        return Database.Scalar <int> ("ActExists", new Dictionary <string, object> () {{"_username", username}}) == 1;
    }

    public static bool ActLogin (this DatabaseConnection Database, string username, string password, out int? accountID, out ulong? role, out bool? banned)
    {
        try
        {
            (accountID, role, banned) = Database.Scalar <int, ulong, bool> (
                "ActLogin", new Dictionary <string, object> ()
                {
                    {"_username", username},
                    {"_password", password}
                }
            );

            return true;
        }
        catch (Exception)
        {
            accountID = null;
            role      = null;
            banned    = null;
            
            return false;
        }
    }

    public static void ActCreate (this DatabaseConnection Database, string username, string password, ulong role)
    {
        Database.Procedure (
            "ActCreate", new Dictionary <string, object> ()
            {
                {"_username", username},
                {"_password", password},
                {"_role", role}
            }
        );
    }
}