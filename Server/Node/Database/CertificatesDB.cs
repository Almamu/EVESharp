using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using Common.Database;
using MySql.Data.MySqlClient;
using Node.Data;
using PythonTypes.Types.Database;

namespace Node.Database
{
    public class CertificatesDB : DatabaseAccessor
    {
        public CertificatesDB(DatabaseConnection db) : base(db)
        {
        }

        public Rowset GetMyCertificates(int characterID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT certificateID, grantDate, visibilityFlags FROM chrCertificates WHERE characterID = @characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
        }

        public List<CertificateRelationship> GetCertificateRequirements(int certificateID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT relationshipID, parentID, parentTypeID, parentLevel, childID, childTypeID FROM crtRelationships WHERE parentID=@certificateID",
                new Dictionary<string, object>()
                {
                    {"@certificateID", certificateID}
                }
            );
            
            using (connection)
            using (reader)
            {
                List<CertificateRelationship> result = new List<CertificateRelationship>();
                
                while (reader.Read() == true)
                {
                    result.Add(new CertificateRelationship()
                        {
                            RelationshipID = reader.GetInt32(0),
                            ParentID = reader.GetInt32(1),
                            ParentTypeID = reader.GetInt32(2),
                            ParentLevel = reader.GetInt32(3),
                            ChildID = reader.GetInt32(4),
                            ChildTypeID = reader.GetInt32(5)
                        }
                    );
                }

                return result;
            }
        }

        public void GrantCertificate(int characterID, int certificateID)
        {
            Database.PrepareQuery(
                "INSERT INTO chrCertificates(characterID, certificateID, grantDate, visibilityFlags)VALUES(@characterID, @certificateID, @grantDate, 0)",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@certificateID", certificateID},
                    {"@grantDate", DateTime.UtcNow.ToFileTimeUtc ()}
                }
            );
        }

        public void UpdateVisibilityFlags(int certificateID, int characterID, int visibilityFlags)
        {
            Database.PrepareQuery(
                "UPDATE chrCertificates SET visibilityFlags=@visibilityFlags WHERE characterID=@characterID AND certificateID=@certificateID",
                new Dictionary<string, object>()
                {
                    {"@visibilityFlags", visibilityFlags},
                    {"@characterID", characterID},
                    {"@certificateID", certificateID}
                }
            );
        }

        public Rowset GetCertificatesByCharacter(int characterID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT certificateID, grantDate, visibilityFlags FROM chrCertificates WHERE characterID = @characterID AND visibilityFlags = 1",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
        }
    }
}