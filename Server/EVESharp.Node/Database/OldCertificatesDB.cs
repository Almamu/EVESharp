using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using EVESharp.Common.Database;
using EVESharp.EVE.StaticData.Certificates;
using EVESharp.PythonTypes.Types.Database;
using MySql.Data.MySqlClient;

namespace EVESharp.Node.Database;

public class OldCertificatesDB : DatabaseAccessor
{
    public OldCertificatesDB (IDatabaseConnection db) : base (db) { }

    /// <summary>
    /// Loads the full list of relationships for the certificates
    /// </summary>
    /// <returns></returns>
    public Dictionary <int, List <Relationship>> GetCertificateRelationships ()
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection, "SELECT relationshipID, parentID, parentTypeID, parentLevel, childID, childTypeID FROM crtRelationships"
        );

        using (connection)
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
    public List <int> GetCertificateListForCharacter (int characterID)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT certificateID FROM chrCertificates WHERE characterID = @characterID",
            new Dictionary <string, object> {{"@characterID", characterID}}
        );

        using (connection)
        using (reader)
        {
            List <int> certificates = new List <int> ();

            while (reader.Read ())
                certificates.Add (reader.GetInt32 (0));

            return certificates;
        }
    }
}