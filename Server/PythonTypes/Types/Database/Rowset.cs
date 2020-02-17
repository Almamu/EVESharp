using MySql.Data.MySqlClient;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Database
{
    public class Rowset
    {
        private const string TYPE_NAME = "util.Rowset";
        private const string ROW_TYPE_NAME = "util.Row";
        
        public static PyDataType FromMySqlDataReader(MySqlDataReader reader)
        {
            if (reader.FieldCount == 0)
                return new PyObjectData(TYPE_NAME, new PyDictionary());
            
            PyDictionary arguments = new PyDictionary();
            PyList header = new PyList();
            
            for(int i = 0; i < reader.FieldCount; i ++)
                header.Add(reader.GetName(i));

            arguments["header"] = header;
            arguments["RowClass"] = new PyToken(ROW_TYPE_NAME);

            PyList rowlist = new PyList();

            while (reader.Read() == true)
            {
                PyList linedata = new PyList();
                
                for(int i = 0; i < reader.FieldCount; i ++)
                    linedata.Add(Utils.ObjectFromColumn(reader, i));

                rowlist.Add(linedata);
            }

            arguments["lines"] = rowlist;
            
            return new PyObjectData(TYPE_NAME, arguments);
        }
    }
}