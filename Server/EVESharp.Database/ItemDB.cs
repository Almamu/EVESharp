using System.Collections.Generic;
using EVESharp.Common.Database;

namespace EVESharp.Database;

public static class ItemDB
{
    public static void InvSetItemNode (this DatabaseConnection Database, int itemID, long nodeID)
    {
        Database.Procedure (
            "InvSetItemNode",
            new Dictionary <string, object>
            {
                {"_itemID", itemID},
                {"_nodeID", nodeID}
            }
        );
    }

    public static long InvGetItemNode (this DatabaseConnection Database, int itemID)
    {
        return Database.Scalar <long> (
            "InvGetItemNode",
            new Dictionary <string, object> {{"_itemID", itemID}}
        );
    }

    public static void InvClearNodeAssociation (this DatabaseConnection Database)
    {
        Database.Procedure ("InvClearNodeAssociation");
    }
}