using System;
using System.Collections.Generic;
using Common.Database;
using MySql.Data.MySqlClient;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Database
{
    public class ConfigDB : DatabaseAccessor
    {
        public ConfigDB(DatabaseConnection db) : base(db)
        {
        }

        public PyDataType GetMultiOwnersEx(PyList ids)
        {
            string query = "SELECT itemID as ownerID, itemName as ownerName, typeID FROM entity WHERE itemID IN (";
            Dictionary<string, object> parameters = new Dictionary<string,object>();

            foreach (PyDataType id in ids)
                parameters["@itemID" + parameters.Count.ToString("X")] = (int) (id as PyInteger);

            // prepare the correct list of arguments
            query += String.Join(',', parameters.Keys) + ")";
            
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection, query, parameters);
            
            using (connection)
            using (reader)
            {
                return TupleSet.FromMySqlDataReader(reader);
            }
        }

        public PyDataType GetMultiGraphicsEx(PyList ids)
        {
            string query = "SELECT graphicID, url3D, urlWeb, icon, urlSound, explosionID FROM eveGraphics WHERE graphicID IN (";
            Dictionary<string, object> parameters = new Dictionary<string,object>();

            foreach (PyDataType id in ids)
                parameters["@graphicID" + parameters.Count.ToString("X")] = (int) (id as PyInteger);

            // prepare the correct list of arguments
            query += String.Join(',', parameters.Keys) + ")";
            
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection, query, parameters);
            
            using (connection)
            using (reader)
            {
                return TupleSet.FromMySqlDataReader(reader);
            }
        }
        
        public PyDataType GetMultiLocationsEx(PyList ids)
        {
            string query = "SELECT itemID as locationID, itemName as locationName, x, y, z FROM entity WHERE itemID IN (";
            Dictionary<string, object> parameters = new Dictionary<string,object>();

            foreach (PyDataType id in ids)
                parameters["@itemID" + parameters.Count.ToString("X")] = (int) (id as PyInteger);

            // prepare the correct list of arguments
            query += String.Join(',', parameters.Keys) + ")";
            
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection, query, parameters);
            
            using (connection)
            using (reader)
            {
                return TupleSet.FromMySqlDataReader(reader);
            }
        }
        
        public PyDataType GetMultiAllianceShortNamesEx(PyList ids)
        {
            string query = "SELECT allianceID, shortName FROM alliance_shortnames WHERE allianceID IN (";
            Dictionary<string, object> parameters = new Dictionary<string,object>();

            foreach (PyDataType id in ids)
                parameters["@itemID" + parameters.Count.ToString("X")] = (int) (id as PyInteger);

            // prepare the correct list of arguments
            query += String.Join(',', parameters.Keys) + ")";
            
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection, query, parameters);
            
            using (connection)
            using (reader)
            {
                return TupleSet.FromMySqlDataReader(reader);
            }
        }
    }
}