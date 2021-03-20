using System;
using MySql.Data.MySqlClient;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Database
{
    public class KeyVal
    {
        private const string OBJECT_NAME = "util.KeyVal";
        /// <summary>
        /// Simple helper method that creates the correct KeyVal data off a result row and
        /// returns it's PyDataType representation, ready to be sent to the EVE Online client
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static PyDataType FromMySqlDataReader(MySqlDataReader reader)
        {
            PyDictionary data = new PyDictionary();

            for (int i = 0; i < reader.FieldCount; i++)
                data[reader.GetName(i)] = Utils.ObjectFromColumn(reader, i);
            
            return new PyObjectData(OBJECT_NAME, data);
        }

        /// <summary>
        /// Simple helper method that creates the correct KeyVal data off a dictionary and
        /// returns it's PyDataType representation, ready to be sent to the EVE Online client
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static PyDataType FromDictionary(PyDictionary columns)
        {
            return new PyObjectData(OBJECT_NAME, columns);
        }

        /// <summary>
        /// Converts a KeyVal to a normal PyDictionary
        /// </summary>
        /// <param name="from">The KeyVal to convert to a dictionary</param>
        /// <returns>The information in the keyval</returns>
        /// <exception cref="InvalidCastException">If the type is not a keyval</exception>
        public static PyDictionary ToDictionary(PyObjectData from)
        {
            if (from.Name != OBJECT_NAME)
                throw new InvalidCastException($"Trying to cast a {from.Name} to a {OBJECT_NAME}");
            
            return from.Arguments as PyDictionary;
        }
    }
}