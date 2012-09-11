using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marshal
{
    // This class is ported from EVEmu
    public class PyObjectEx_Type1 : PyObjectEx
    {
        // new is used to get ridd of the odd warning
        public new PyToken Type
        {
            get
            {
                return Header.As<PyTuple>()[0].As<PyToken>();
            }
        }

        public PyTuple Args
        {
            get
            {
                return Header.As<PyTuple>()[1].As<PyTuple>();
            }
        }

        public PyDict Keywords
        {
            get
            {
                PyTuple t = Header as PyTuple;

                if (t.Items.Count < 3)
                    t.Items.Insert(2, new PyDict());

                return t[2] as PyDict;
            }
        }

        public PyObjectEx_Type1(PyToken type, PyTuple args)
            : base(false, CreateHeader(type, args))
        {

        }

        private static PyTuple CreateHeader(PyToken type, PyTuple args)
        {
            if (args == null)
                args = new PyTuple();

            PyTuple head = new PyTuple();

            head.Items.Add(type);
            head.Items.Add(args);

            return head;
        }

        public PyObject FindKeyword(string keyword)
        {
            PyDict kw = Keywords;

            foreach (var pair in kw.Dictionary)
            {
                if (pair.Key is PyString)
                {
                    if (pair.Key.StringValue == keyword)
                        return pair.Value;
                }
            }

            return null;
        }
    }
}
