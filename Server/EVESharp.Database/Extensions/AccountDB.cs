using System;
using System.Collections.Generic;

namespace EVESharp.Database.Extensions;

public static class AccountDB
{
    public static long CluResolveClientAddress (this IDatabase Database, int clientID)
    {
        return Database.SelectProcedure (
            "CluResolveClientAddress",
            new Dictionary <string, object> {{"_clientID", clientID}}
        ).Scalar <long> ();
    }

    public static void CluRegisterClientAddress (this IDatabase Database, int clientID, long proxyNodeID)
    {
        Database.QueryProcedure (
            "CluRegisterClientAddress",
            new Dictionary <string, object>
            {
                {"_clientID", clientID},
                {"_proxyNodeID", proxyNodeID}
            }
        );
    }

    public static void CluResetClientAddresses (this IDatabase Database)
    {
        Database.QueryProcedure ("CluResetClientAddresses");
    }

    public static bool ActExists (this IDatabase Database, string username)
    {
        return Database
           .SelectProcedure ("ActExists", new Dictionary <string, object> () {{"_username", username}})
           .Scalar <int> () == 1;
    }

    public static bool ActLogin (this IDatabase Database, string username, string password, out int? accountID, out ulong? role, out bool? banned)
    {
        try
        {
            (accountID, role, banned) = Database.SelectProcedure (
                "ActLogin", new Dictionary <string, object> ()
                {
                    {"_username", username},
                    {"_password", password}
                }
            ).Scalar <int, ulong, bool> ();

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

    public static void ActCreate (this IDatabase Database, string username, string password, ulong role)
    {
        Database.QueryProcedure (
            "ActCreate", new Dictionary <string, object> ()
            {
                {"_username", username},
                {"_password", password},
                {"_role", role}
            }
        );
    }
}