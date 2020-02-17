using System;
using System.Collections.Generic;
using System.IO;
using MySql.Data.MySqlClient;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Database
{
    public class PyPackedRow : PyDataType
    {
        public DBRowDescriptor Header { get; private set; }

        private Dictionary<string, PyDataType> mValues = new Dictionary<string, PyDataType>();
        
        public PyPackedRow(DBRowDescriptor descriptor) : base(PyObjectType.PackedRow)
        {
            this.Header = descriptor;
        }

        public PyPackedRow(DBRowDescriptor descriptor, PyDataType[] values) : base(PyObjectType.PackedRow)
        {
            this.Header = descriptor;
            
            if(values.Length != this.Header.Columns.Count)
                throw new Exception("PackedRow must have the same value count as DBRowDescriptor");
            
            int index = 0;
            
            foreach (DBRowDescriptor.Column column in descriptor.Columns)
            {
                switch (column.Type)
                {
                    case FieldType.I1:
                    case FieldType.I2:
                    case FieldType.I4:
                    case FieldType.I8:
                    case FieldType.UI1:
                    case FieldType.UI2:
                    case FieldType.UI4:
                    case FieldType.UI8:
                    case FieldType.FileTime:
                    case FieldType.CY:
                        this.mValues.Add(column.Name, values[index++] as PyInteger);
                        break;
                    
                    case FieldType.Bool:
                        this.mValues.Add(column.Name, values[index++] as PyBool);
                        break;
                    
                    case FieldType.Bytes:
                        this.mValues.Add(column.Name, values[index++] as PyBuffer);
                        break;
                    
                    case FieldType.WStr:
                    case FieldType.Str:
                        this.mValues.Add(column.Name, values[index++] as PyString);
                        break;
                    
                    case FieldType.Null:
                        this.mValues.Add(column.Name, values[index++] as PyNone);
                        break;
                    
                    default:
                        throw new Exception($"Unknown column type {column.Type}");
                }
            }
        }
        
        public virtual PyDataType this[string key]
        {
            get { return this.mValues[key]; }
            set { this.mValues[key] = value; }
        }

        public static PyPackedRow FromMySqlDataReader(MySqlDataReader reader, DBRowDescriptor descriptor)
        {
            PyPackedRow row = new PyPackedRow(descriptor);

            int i = 0;
            
            foreach(DBRowDescriptor.Column column in descriptor.Columns)
            {
                row[column.Name] = Utils.ObjectFromColumn(reader, i++);
            }
            
            return row;
        }

        public static PyPackedRow FromMySqlDataReader(MySqlDataReader reader, CRowset rowset)
        {
            return FromMySqlDataReader(reader, rowset.Header);
        }
    }
}