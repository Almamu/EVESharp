using System;
using MySql.Data.MySqlClient;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Database
{
    public class TupleSet
    {
        public static PyDataType FromMySqlDataReader(MySqlDataReader reader)
        {
            if (reader.FieldCount == 0)
                return new PyTuple(0);

            PyList columns = new PyList(reader.FieldCount);
            PyList rows = new PyList();
            
            for(int i = 0; i < columns.Count; i ++)
                columns[i] = new PyString(reader.GetName(i));

            while (reader.Read() == true)
            {
                PyList linedata = new PyList(columns.Count);

                for (int i = 0; i < columns.Count; i++)
                    linedata[i] = Utils.ObjectFromColumn(reader, i);

                rows.Add(linedata);
            }

            return new PyTuple(new PyDataType[]
            {
                columns, rows
            });
        }
    }
}