using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Database
{
    public class DBRowDescriptor
    {
        public class Column
        {
            public string Name { get; }
            public FieldType Type { get; }

            public Column(string name, FieldType type)
            {
                this.Name = name;
                this.Type = type;
            }

            public Column(string name, int type)
            {
                this.Name = name;
                this.Type = (FieldType) type;
            }

            public static implicit operator PyDataType(Column column)
            {
                return new PyTuple(new PyDataType[]
                {
                    column.Name,
                    (int) column.Type
                });
            }

            public static implicit operator Column(PyDataType column)
            {
                PyTuple tuple = column as PyTuple;

                return new Column(
                    tuple[0] as PyString,
                    tuple[1] as PyInteger
                );
            }
        };

        private const string TYPE_NAME = "blue.DBRowDescriptor";

        public List<Column> Columns { get; }

        public DBRowDescriptor()
        {
            this.Columns = new List<Column>();
        }

        public static implicit operator PyObject(DBRowDescriptor descriptor)
        {
            PyTuple args = new PyTuple(descriptor.Columns.Count);
            int index = 0;

            foreach (Column col in descriptor.Columns)
                args[index++] = col;

            args = new PyTuple(new PyDataType[] {args});
            // build the args tuple
            return new PyObject(
                false,
                new PyTuple(new PyDataType[] { new PyToken(TYPE_NAME), args })
            );
        }

        public static implicit operator DBRowDescriptor(PyObject descriptor)
        {
            if(descriptor.Header[0] is PyToken == false || descriptor.Header[0] as PyToken != TYPE_NAME)
                throw new Exception($"Expected PyObject of type {TYPE_NAME}");

            DBRowDescriptor output = new DBRowDescriptor();

            foreach(PyTuple tuple in descriptor.Header[1] as PyTuple)
                output.Columns.Add(tuple);

            return output;
        }

        public static implicit operator DBRowDescriptor(PyDataType descriptor)
        {
            return descriptor as PyObject;
        }

        public static DBRowDescriptor FromMySqlReader(MySqlDataReader reader)
        {
            DBRowDescriptor descriptor = new DBRowDescriptor();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                Type type = reader.GetFieldType(i);
                FieldType fieldType = FieldType.Error;

                if (type == typeof(String))
                    // TODO: PROPERLY SPECIFY THE STRING TYPE
                    fieldType = FieldType.WStr;
                else if (type == typeof(UInt64))
                    fieldType = FieldType.UI8;
                else if (type == typeof(Int64))
                    fieldType = FieldType.I8;
                else if (type == typeof(UInt32))
                    fieldType = FieldType.UI4;
                else if (type == typeof(Int32))
                    fieldType = FieldType.I4;
                else if (type == typeof(UInt16))
                    fieldType = FieldType.UI2;
                else if (type == typeof(Int16))
                    fieldType = FieldType.I2;
                else if (type == typeof(SByte))
                    fieldType = FieldType.I1;
                else if (type == typeof(Byte))
                    fieldType = FieldType.UI1;
                else if (type == typeof(Byte[]))
                    fieldType = FieldType.Bytes;
                else if (type == typeof(Double))
                    fieldType = FieldType.R8;
                else if (type == typeof(float))
                    fieldType = FieldType.R4;
                else if (type == typeof(Boolean))
                    fieldType = FieldType.Bool;
                else
                    throw new Exception("Unknown field type");

                descriptor.Columns.Add(
                    new Column(reader.GetName(i), fieldType)
                );
            }

            return descriptor;
        }
    }
}