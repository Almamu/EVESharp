using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal.Network;

namespace Marshal
{
    // Ported from EVEmu: The EVE Online emulator
    // Just a helper class to help us reply to calls
    public class PyPacket : Encodeable, Decodeable
    {
        public PyPacket()
        {
            type = Macho.MachoNetMsg_Type.__Fake_Invalid_Type;
            type_string = "none";
            userID = 0;
            payload = null;
            named_payload = null;
            source = new PyAddress();
            dest = new PyAddress();
        }

        public PyObject Encode()
        {
            PyTuple args = new PyTuple();

            // Command
            args.Items.Add(new PyInt((int)type));
            
            // Source
            args.Items.Add(source.Encode());

            // Dest
            args.Items.Add(dest.Encode());

            // unknown
            if (userID == 0)
            {
                args.Items.Add(new PyNone());
            }
            else
            {
                args.Items.Add(new PyInt((int)userID));
            }

            // Add call args( payload )
            args.Items.Add(payload);

            // Named arguments
            if (named_payload == null)
            {
                args.Items.Add(new PyNone());
            }
            else
            {
                args.Items.Add(named_payload);
            }

            return new PyObjectData(type_string, args);
        }

        public void Decode( PyObject data )
        {
            PyObject packet = data;

            if (packet.Type == PyObjectType.ChecksumedStream)
            {
                packet = packet.As<PyChecksumedStream>().Data;
            }

            if (packet.Type == PyObjectType.SubStream)
            {
                packet = packet.As<PySubStream>().Data;
            }

            if (packet.Type != PyObjectType.ObjectData)
            {
                throw new Exception($"Expected container to be of type ObjectData but got {packet.Type}");
            }

            PyObjectData packeto = packet.As<PyObjectData>();

            type_string = packeto.Name;

            if (packeto.Arguments.Type != PyObjectType.Tuple)
            {
                throw new Exception($"Expected arguments to be of type Tuple but got {packeto.Arguments.Type}");
            }
            
            PyTuple tuple = packeto.Arguments.As<PyTuple>();

            if (tuple.Items.Count != 6)
            {
                throw new Exception($"Expected arguments to be of size 6 but got {tuple.Items.Count}");
            }

            if ((tuple.Items[0].Type != PyObjectType.IntegerVar) && ( tuple.Items[0].Type != PyObjectType.Long ) )
            {
                throw new Exception($"Expected item 1 to be of type Long or Integer but got {tuple.Items[0].Type}");
            }

            PyInt typer = tuple.Items[0].As<PyInt>();

            type = (Macho.MachoNetMsg_Type)typer.Value;

            source.Decode(tuple.Items[1]);
            dest.Decode(tuple.Items[2]);

            if (tuple.Items[3].Type == PyObjectType.IntegerVar)
            {
                userID = (tuple.Items[3] as PyIntegerVar).Value;
            }
            else if (tuple.Items[3].Type == PyObjectType.Long)
            {
                userID = tuple.Items[3].As<PyInt>().Value;
            }
            else if (tuple.Items[3].Type == PyObjectType.LongLong)
            {
                userID = (tuple.Items[3] as PyLongLong).Value;
            }
            else if (tuple.Items[3].Type == PyObjectType.None)
            {
                userID = 0;
            }
            else
            {
                throw new Exception($"Expected item 4 to be of type Integer, Long or None but got {tuple.Items[3].Type}");
            }

            // Payload( or call arguments )
            if ((tuple.Items[4].Type != PyObjectType.Buffer) && (tuple.Items[4].Type != PyObjectType.Tuple))
            {
                throw new Exception($"Expected item 5 to be of type Buffer or Tuple but got {tuple.Items[4].Type}");
            }

            payload = tuple.Items[4].As<PyTuple>();

            if (tuple.Items[5].Type == PyObjectType.None)
            {
                named_payload = new PyDict();
            }
            else if (tuple.Items[5].Type == PyObjectType.Dict)
            {
                named_payload = tuple.Items[5].As<PyDict>();
            }
            else
            {
                throw new Exception($"Expected item 6 to be of type None or Dict but got {tuple.Items[5].Type}");
            }
        }
        
        public Macho.MachoNetMsg_Type type;
        public PyAddress source;
        public PyAddress dest;
        public long userID;
        public PyTuple payload;
        public PyDict named_payload;
        public string type_string;
    }
}
