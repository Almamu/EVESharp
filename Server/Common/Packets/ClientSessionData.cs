using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;
using Marshal.Network;

namespace Common.Packets
{
    class ClientSessionData : Encodeable, Decodeable
    {
        public PyDict session = new PyDict();
        public int clientID = 0;

        public PyObject Encode()
        {
            PyTuple args = new PyTuple();

            args.Items.Add(session);
            args.Items.Add(new PyInt(clientID));

            return new PyObjectData("macho.sessionInitialState", args);
        }

        public void Decode(PyObject data)
        {
            if (data.Type != PyObjectType.ObjectData)
            {
                throw new Exception($"Expected container of type ObjectData but got {data.Type}");
            }

            PyObjectData container = data as PyObjectData;

            if (container.Name != "macho.sessionInitialState")
            {
                throw new Exception($"Expected container with typeName 'macho.sessionInitialState' but got '{container.Name}'");
            }

            if (container.Arguments.Type != PyObjectType.Tuple)
            {
                throw new Exception($"Expected arguments of type Tuple but got {container.Arguments.Type}");
            }

            PyTuple args = container.Arguments as PyTuple;

            if (args.Items.Count != 2)
            {
                throw new Exception($"Expected arguments with 2 arguments but got {args.Items.Count}");
            }

            if (args.Items[0].Type != PyObjectType.Dict)
            {
                throw new Exception($"Expected PyDict as first argument but got {args.Items[0].Type}");
            }

            session = args.Items[0] as PyDict;

            if ((args.Items[1] is PyInt) == false)
            {
                throw new Exception($"Expected PyInt as second argument but got {args.Items[1].Type}");
            }

            clientID = (args.Items[1] as PyInt).Value;
        }
    }
}
