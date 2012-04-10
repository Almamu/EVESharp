using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;

namespace Common.Packets
{
    class ClientSessionData
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

        public bool Decode(PyObject data)
        {
            if (data.Type != PyObjectType.ObjectData)
            {
                Log.Error("ClientSessionData", "Wrong container type");
                return false;
            }

            PyObjectData container = data as PyObjectData;

            if (container.Name != "macho.sessionInitialState")
            {
                Log.Error("ClientSessionData", "Wrong container name/type");
                return false;
            }

            PyTuple args = container.Arguments as PyTuple;

            if (args.Items.Count != 2)
            {
                Log.Error("ClientSessionData", "Wrong args count");
                return false;
            }

            if (args.Items[0].Type != PyObjectType.Dict)
            {
                Log.Error("ClientSessionData", "Arguments first element is not PyDict");
                return false;
            }

            session = args.Items[0] as PyDict;

            if ((args.Items[1] is PyInt) == false)
            {
                Log.Error("ClientSessionData", "Arguments second element is not PyInt");
                return false;
            }

            clientID = (args.Items[1] as PyInt).Value;

            return true;
        }
    }
}
