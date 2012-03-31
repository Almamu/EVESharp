using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;

namespace Common.Packets
{
    public class SessionChangeNotification
    {
        public int clueless = 0;
        public PyDict changes = new PyDict();
        public PyList nodesOfInterest = new PyList();

        public PyObject Encode()
        {
            PyTuple res = new PyTuple();

            PyTuple main = new PyTuple();

            main.Items.Add(new PyInt(clueless));
            main.Items.Add(changes);

            res.Items.Add(main);
            res.Items.Add(nodesOfInterest);

            return res.As<PyObject>();
        }
    }
}
