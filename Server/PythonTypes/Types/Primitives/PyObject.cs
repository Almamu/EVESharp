namespace PythonTypes.Types.Primitives
{
    public class PyObject : PyDataType
    {
        public class ObjectHeader
        {
            public PyToken Type { get; }
            public PyTuple Arguments { get; }

            public PyDictionary Dictionary { get; }

            // TODO: GET RID OF THIS FLAG OR MAKE IT MORE COMPREHENSIBLE
            public bool IsType2 { get; }

            public ObjectHeader(PyToken type, PyTuple args, PyDictionary dictionary, bool isType2 = false)
            {
                this.Type = type;
                this.Arguments = args;
                this.Dictionary = dictionary;
                this.IsType2 = isType2;
            }

            public static implicit operator PyDataType(ObjectHeader header)
            {
                PyTuple finalArguments = null;

                if (header.IsType2 == true)
                {
                    if (header.Arguments != null)
                    {
                        if(header.Dictionary == null)
                            finalArguments = new PyTuple(new PyDataType[] { header.Type, header.Arguments });
                        else
                            finalArguments = new PyTuple(new PyDataType[] { header.Type, header.Arguments, header.Dictionary });
                    }
                    else
                    {
                        if(header.Dictionary == null)
                            finalArguments = new PyTuple(new PyDataType[] { new PyTuple (new PyDataType[] { header.Type }) });
                        else
                            finalArguments = new PyTuple(new PyDataType[] { new PyTuple (new PyDataType[] { header.Type }), header.Dictionary });
                    }
                }
                else
                {
                    if (header.Arguments == null)
                    {
                        if(header.Dictionary == null)
                            finalArguments = new PyTuple(new PyDataType[] { header.Type });
                        else
                            finalArguments = new PyTuple(new PyDataType[] { header.Type, header.Dictionary });
                    }
                    else
                    {
                        int length = header.Arguments.Count + 1;
                        if (header.Dictionary != null)
                            length++;

                        finalArguments = new PyTuple(length);
                        finalArguments[0] = header.Type;

                        // copy arguments over
                        header.Arguments.CopyTo(finalArguments, 0, 1);
                        // put dictionary in
                        if (header.Dictionary != null)
                            finalArguments[finalArguments.Count - 1] = header.Dictionary;
                    }
                }

                return finalArguments;
            }

            public static implicit operator ObjectHeader(PyTuple header)
            {
                PyToken type = null;
                PyTuple arguments = null;
                PyDictionary dictionary = null;
                bool isType2 = false;

                if (header[0] is PyToken)
                {
                    type = header[0] as PyToken;
                    arguments = header[1] as PyTuple;

                    if (header.Count == 3)
                        dictionary = header[2] as PyDictionary;
                }
                else if (header[0] is PyTuple)
                {
                    arguments = header[0] as PyTuple;
                    dictionary = header[1] as PyDictionary;

                    // take out the type token and build the correct arguments tuple
                    type = arguments[0] as PyToken;

                    if (arguments.Count > 1)
                    {
                        PyTuple tmp = new PyTuple(arguments.Count - 1);
                        arguments.CopyTo(tmp, 1, 0);
                        arguments = tmp;
                    }
                    else
                    {
                        arguments = null;
                    }

                    isType2 = true;
                }

                return new ObjectHeader(type, arguments, dictionary, isType2);
            }
        }

        public ObjectHeader Header { get; }
        public PyList List { get; }
        public PyDictionary Dictionary { get; }

        public PyObject(PyToken type, PyTuple args, PyDictionary keywords = null) : base(PyObjectType.Object)
        {
            this.Header = new ObjectHeader(type, args, keywords);
            this.List = new PyList();
            this.Dictionary = new PyDictionary();
        }

        public PyObject(ObjectHeader header, PyList list, PyDictionary dictionary = null) : base(PyObjectType.Object)
        {
            this.Header = header;
            this.List = list;
            this.Dictionary = dictionary ?? new PyDictionary();
        }
    }
}