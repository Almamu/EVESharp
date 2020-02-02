using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;
using Marshal.Database;

namespace Common.Utils
{
    public class CRowset
    {
        private PyList items = new PyList();
        public DBRowDescriptor descriptor = null;

        public CRowset(DBRowDescriptor db)
        {
            descriptor = db;
        }

        public void Insert(PyPackedRow row)
        {
            row.Header = descriptor;
            items.Items.Add(row);
        }

        public PyObject Encode()
        {
            PyTuple args = new PyTuple();
            args.Items.Add(new PyToken("dbutil.CRowset"));

            PyDict dict = new PyDict();
            dict.Set("header", descriptor);

            PyList columns = new PyList();
            
            for(int i = 0; i < descriptor.ColumnCount; i ++)
                columns.Items.Add(descriptor.GetColumnName(i));
            
            dict.Set("columns", columns);

            PyObjectEx res = new PyObjectEx();
            res.IsType2 = true;

            PyTuple header = new PyTuple();
            header.Items.Add(args);
            header.Items.Add(dict);
            
            res.List = items.Items;
            res.Header = header;
            
            return res;
        }
    }
}
