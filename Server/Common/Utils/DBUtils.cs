using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;
using Marshal.Database;
using MySql.Data.Types;
using MySql.Data.MySqlClient;

namespace Common.Utils
{
    public static class DBUtils
    {
        public static PyObject DBColumnToPyObject(int index, ref MySqlDataReader reader)
        {
            Type type = reader.GetFieldType(index);

            switch (type.Name)
            {
                case "String":
                    return new PyString(reader.IsDBNull(index) == true ? "" : reader.GetString(index));
                case "UInt32":
                case "Int32":
                case "UInt16":
                case "Int16":
                case "SByte":
                case "Byte":
                    return new PyInt(reader.IsDBNull(index) == true ? 0 : reader.GetInt32(index));
                case "UInt64":
                case "Int64":
                    return new PyLongLong(reader.IsDBNull(index) == true ? 0 : reader.GetInt64(index));
                case "Byte[]":
                    return new PyBuffer(reader.IsDBNull(index) == true ? new byte[0] : (byte[])reader.GetValue(index));
                case "Double":
                    return new PyFloat(reader.IsDBNull(index) == true ? 0.0 : reader.GetDouble(index));
                case "Decimal":
                    return new PyFloat(reader.IsDBNull(index) == true ? 0.0 : (double)reader.GetDecimal(index));
                case "Boolean":
                    return new PyBool(reader.IsDBNull(index) == true ? false : reader.GetBoolean(index));
                default:
                    Log.Error("Database", "Unhandled MySQL type " + type.Name);
                    break;
            }

            return null;
        }

        public static PyObject DBResultToRowset(ref MySqlDataReader dat)
        {
            int column = dat.FieldCount;
            if (column == 0)
            {
                return new PyObjectData("util.Rowset", new PyDict());
            }

            PyDict args = new PyDict();

            PyList header = new PyList();
            for (int i = 0; i < column; i++)
            {
                header.Items.Add(new PyString(dat.GetName(i)));
            }

            args.Set("header", header);

            args.Set("RowClass", new PyToken("util.Row"));

            PyList rowlist = new PyList();

            while (dat.Read())
            {
                PyList linedata = new PyList();
                for (int r = 0; r < column; r++)
                {
                    linedata.Items.Add(DBColumnToPyObject(r, ref dat));
                }

                rowlist.Items.Add(linedata);
            }

            dat.Close();

            return new PyObjectData("util.Rowset", args);
        }

        public static PyObject DBResultToTupleSet(ref MySqlDataReader result)
        {
            int column = result.FieldCount;
            if (column == 0)
                return new PyTuple();

            int r = 0;

            PyTuple res = new PyTuple();
            PyList cols = new PyList();
            PyList reslist = new PyList();

            for (r = 0; r < column; r++)
            {
                cols.Items.Add(new PyString(result.GetName(r)));
            }

            while (result.Read())
            {
                PyList linedata = new PyList();
                for (r = 0; r < column; r++)
                {
                    linedata.Items.Add(DBColumnToPyObject(r, ref result));
                }

                reslist.Items.Add(linedata);
            }

            res.Items.Add(cols);
            res.Items.Add(reslist);

            result.Close();

            return res;
        }

        public static PyObject DBResultToCRowset(ref MySqlDataReader result)
        {
            DBRowDescriptor header = new DBRowDescriptor(ref result);
            CRowset rowset = new CRowset(header);

            while (result.Read())
            {
                rowset.Insert(CreatePackedRow(header, ref result));
            }

            result.Close();

            return rowset.Encode();
        }

        public static PyPackedRow CreatePackedRow(DBRowDescriptor header, ref MySqlDataReader result)
        {
            PyPackedRow row = new PyPackedRow(header);
            for (int i = 0; i < header.ColumnCount; i++)
            {
                row.SetValue(header.GetColumnName(i).StringValue, DBColumnToPyObject(i, ref result));
            }
            return row;
        }

        public static PyObject DBResultToPackedRowList(ref MySqlDataReader result)
        {
            PyList res = new PyList();
            DBRowDescriptor header = new DBRowDescriptor(ref result);

            while (result.Read())
            {
                res.Items.Add(CreatePackedRow(header, ref result));
            }

            return res;
        }
    }
}
