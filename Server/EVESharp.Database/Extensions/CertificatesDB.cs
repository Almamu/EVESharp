using System;
using System.Collections.Generic;
using System.Data.Common;
using EVESharp.Database.Certificates;
using EVESharp.Database.Types;

namespace EVESharp.Database.Extensions;

public static class CertificatesDB
{
    public static Rowset CrtGetCharacterCertificates (this IDatabase Database, int characterID)
    {
        return Database.Rowset ("CrtGetCharacterCertificates", new Dictionary <string, object> () {{"_characterID", characterID}});
    }

    public static void CrtUpdateVisibilityFlags (this IDatabase Database, int characterID, int certificateID, int flags)
    {
        Database.Query (
            "CrtUpdateVisibilityFlags",
            new Dictionary <string, object> ()
            {
                {"_characterID", characterID},
                {"_certificateID", certificateID},
                {"_flags", flags}
            }
        );
    }

    public static void CrtGrantCertificate (this IDatabase Database, int characterID, int certificateID)
    {
        Database.Query (
            "CrtGrantCertificate",
            new Dictionary<string, object> ()
            {
                {"_characterID", characterID},
                {"_certificateID", certificateID},
                {"_grantDate", DateTime.Now.ToFileTimeUtc()}
            }
        );
    }
    
    public static Dictionary <int, List <Relationship>> GetCertificateRelationships (this IDatabase Database)
    {
        DbDataReader reader = Database.Select (
            "SELECT relationshipID, parentID, parentTypeID, parentLevel, childID, childTypeID FROM crtRelationships"
        );

        using (reader)
        {
            Dictionary <int, List <Relationship>> result = new Dictionary <int, List <Relationship>> ();

            while (reader.Read ())
            {
                if (result.TryGetValue (reader.GetInt32 (4), out List <Relationship> relationships) == false)
                    relationships = result [reader.GetInt32 (4)] = new List <Relationship> ();

                relationships.Add (
                    new Relationship
                    {
                        RelationshipID = reader.GetInt32 (0),
                        ParentID       = reader.GetInt32 (1),
                        ParentTypeID   = reader.GetInt32 (2),
                        ParentLevel    = reader.GetInt32 (3),
                        ChildID        = reader.GetInt32 (4),
                        ChildTypeID    = reader.GetInt32 (5)
                    }
                );
            }

            return result;
        }
    }

    /// <summary>
    /// Obtains the list of certificates a charactert has
    /// </summary>
    /// <param name="characterID"></param>
    /// <returns></returns>
    public static List <int> GetCertificateListForCharacter (this IDatabase Database, int characterID)
    {
        DbDataReader reader = Database.Select (
            "SELECT certificateID FROM chrCertificates WHERE characterID = @characterID",
            new Dictionary <string, object> {{"@characterID", characterID}}
        );

        using (reader)
        {
            List <int> certificates = new List <int> ();

            while (reader.Read ())
                certificates.Add (reader.GetInt32 (0));

            return certificates;
        }
    }
}