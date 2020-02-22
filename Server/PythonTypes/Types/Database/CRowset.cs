using System.IO;
using MySql.Data.MySqlClient;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Database
{
    /// <summary>
    /// Helper class to work with dbutil.CRowset types to be sent to the EVE Online client
    /// </summary>
    public class CRowset
    {
        private const string TYPE_NAME = "dbutil.CRowset";
        public DBRowDescriptor Header { get; private set; }
        private PyList Columns { get; set; }
        private PyList Rows { get; set; }

        public CRowset(DBRowDescriptor descriptor)
        {
            this.Header = descriptor;
            this.Rows = new PyList();

            this.PrepareColumnNames();
        }

        public CRowset(DBRowDescriptor descriptor, PyList rows)
        {
            this.Header = descriptor;
            this.Rows = rows;

            this.PrepareColumnNames();
        }

        /// <summary>
        /// Creates the columns list based off the DBRowDescriptor of the header
        /// </summary>
        private void PrepareColumnNames()
        {
            this.Columns = new PyList();

            foreach (DBRowDescriptor.Column column in this.Header.Columns)
                this.Columns.Add(column.Name);
        }

        public virtual PyPackedRow this[int index]
        {
            get => this.Rows[index] as PyPackedRow;
            set => this.Rows[index] = value;
        }

        /// <summary>
        /// Adds a new <seealso cref="PyPackedRow" /> to the result data of the CRowset
        /// </summary>
        /// <param name="row">The new row to add</param>
        public void Add(PyPackedRow row)
        {
            this.Rows.Add(row);
        }

        public static implicit operator PyDataType(CRowset rowset)
        {
            PyDictionary keywords = new PyDictionary();

            keywords["header"] = rowset.Header;
            keywords["columns"] = rowset.Columns;

            return new PyObject(
                true,
                new PyTuple(new PyDataType[] { new PyTuple(new PyDataType[] { new PyToken(TYPE_NAME) }), keywords }),
                rowset.Rows
            );
        }

        public static implicit operator CRowset(PyObject data)
        {
            if(data.Header[0] is PyToken == false || data.Header[0] as PyToken != TYPE_NAME)
                throw new InvalidDataException($"Expected PyObject of type {data}");

            DBRowDescriptor descriptor = (data.Header[1] as PyDictionary)["header"];

            return new CRowset(descriptor, data.List);
        }

        /// <summary>
        /// Helper method to instantiate a dbutil.CRowset type from a MySqlDataReader, this consumes the result
        /// but does not close it, so calling code has to take care of this. Ideally, please use "using" statements
        /// </summary>
        /// <param name="reader">The reader to use as source of the information</param>
        /// <returns>The CRowset object ready to be used</returns>
        public static CRowset FromMySqlDataReader(MySqlDataReader reader)
        {
            DBRowDescriptor descriptor = DBRowDescriptor.FromMySqlReader(reader);
            CRowset rowset = new CRowset(descriptor);

            while (reader.Read() == true)
                rowset.Add(PyPackedRow.FromMySqlDataReader(reader, descriptor));

            return rowset;
        }
    }
}