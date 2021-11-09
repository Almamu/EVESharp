using System;
using System.Collections.Generic;
using EVESharp.PythonTypes.Types.Primitives;
using MySql.Data.MySqlClient;

namespace EVESharp.PythonTypes.Types.Database
{
    /// <summary>
    /// Extended Python type that represents a normal row in the database that is compressed when marshaled
    /// </summary>
    public class PyPackedRow : PyDataType
    {
        /// <summary>
        /// The header for this PyPackedRow
        /// </summary>
        public DBRowDescriptor Header { get; }

        private readonly Dictionary<string, PyDataType> mValues = new Dictionary<string, PyDataType>();

        public PyPackedRow(DBRowDescriptor descriptor)
        {
            this.Header = descriptor;
        }

        public PyPackedRow(DBRowDescriptor descriptor, Dictionary<string, PyDataType> values)
        {
            this.Header = descriptor;

            if (values.Count != this.Header.Columns.Count)
                throw new Exception("PackedRow must have the same value count as DBRowDescriptor");

            this.mValues = values;
        }

        public virtual PyDataType this[string key]
        {
            get => this.mValues[key];
            set => this.mValues[key] = value;
        }

        /// <summary>
        /// Simple helper method that creates the correct PackedRow data off a result row and
        /// returns it's PyDataType representation, ready to be sent to the EVE Online client
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        public static PyPackedRow FromMySqlDataReader(MySqlDataReader reader, DBRowDescriptor descriptor)
        {
            PyPackedRow row = new PyPackedRow(descriptor);

            int i = 0;

            foreach (DBRowDescriptor.Column column in descriptor.Columns)
                row[column.Name] = IDatabaseConnection.ObjectFromColumn(reader, column.Type, i++);

            return row;
        }

        public static PyPackedRow FromMySqlDataReader(MySqlDataReader reader, CRowset rowset)
        {
            return FromMySqlDataReader(reader, rowset.Header);
        }
    }
}