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
            string query = "SELECT itemName as ownerName, itemID as ownerID, typeID FROM entity WHERE itemID IN (";
            Dictionary<string, object> parameters = new Dictionary<string,object>();

            foreach (PyDataType id in ids)
            {
                string parameterName = "@itemID" + parameters.Count.ToString("X");
                
                query += parameterName;
                parameters[parameterName] = id as PyInteger;
            }

            query += ")";
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