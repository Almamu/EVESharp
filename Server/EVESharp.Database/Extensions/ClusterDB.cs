using System.Collections.Generic;

namespace EVESharp.Database.Extensions;

public static class ClusterDB
{
    public static long CluResolveAddress (this IDatabase Database, string type, int objectID)
    {
        return Database.Scalar <long> (
            "CluResolveAddress", new Dictionary <string, object> ()
            {
                {"_type", type},
                {"_objectID", objectID}
            }
        );
    }

    public static void CluRegisterSingleNode (this IDatabase Database, long nodeID)
    {
        Database.QueryProcedure ("CluRegisterSingleNode", new Dictionary <string, object> () {{"_nodeID", nodeID}});
    }

    public static void CluCleanup (this IDatabase Database)
    {
        Database.QueryProcedure ("CluCleanup");
    }

    public static int CluResolveCharacter (this IDatabase Database, int characterID)
    {
        return (int) Database.Scalar <uint> ("CluResolveCharacter", new Dictionary <string, object> () {{"_characterID", characterID}});
    }
}