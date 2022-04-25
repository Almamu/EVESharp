using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EVESharp.Common.Database;
using EVESharp.PythonTypes.Types.Database;

namespace EVESharp.Database;

public static class ClusterDB
{
    public static long CluResolveAddress (this IDatabaseConnection Database, string type, int objectID)
    {
        return Database.Scalar <long> (
            "CluResolveAddress", new Dictionary <string, object> ()
            {
                {"_type", type},
                {"_objectID", objectID}
            }
        );
    }

    public static void CluRegisterSingleNode (this IDatabaseConnection Database, long nodeID)
    {
        Database.Procedure ("CluRegisterSingleNode", new Dictionary <string, object> () {{"_nodeID", nodeID}});
    }

    public static void CluCleanup (this IDatabaseConnection Database)
    {
        Database.Procedure ("CluCleanup");
    }

    public static int CluResolveCharacter (this IDatabaseConnection Database, int characterID)
    {
        return (int) Database.Scalar <uint> ("CluResolveCharacter", new Dictionary <string, object> () {{"_characterID", characterID}});
    }
}