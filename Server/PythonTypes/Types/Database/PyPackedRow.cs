using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Database
{
    public class PyPackedRow : PyDataType
    {
        public DBRowDescriptor Header { get; private set; }

        private readonly Dictionary<string, PyDataType> mValues = new Dictionary<string, PyDataType>();

        public PyPackedRow(DBRowDescriptor descriptor) : base(PyObjectType.PackedRow)
        {
            this.Header = descriptor;
        }

        public PyPackedRow(DBRowDescriptor descriptor, Dictionary<string, PyDataType> values) : base(PyObjectType.PackedRow)
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

        public static PyPackedRow FromMySqlDataReader(MySqlDataReader reader, DBRowDescriptor descriptor)
        {
            PyPackedRow row = new PyPackedRow(descriptor);

            int i = 0;

            foreach (DBRowDescriptor.Column column in descriptor.Columns)
                row[column.Name] = Utils.ObjectFromColumn(reader, i++);

            return row;
        }

        public static PyPackedRow FromMySqlDataReader(MySqlDataReader reader, CRowset rowset)
        {
            return FromMySqlDataReader(reader, rowset.Header);
        }
    }
}