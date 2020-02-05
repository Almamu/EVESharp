using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal.Network;

namespace Marshal
{
    // Ported from EVEmu: The EVE Online emulator
    // This class eases the process of reading the macho.MachoAddress PyObjectData
    // To create a easy reply

    public class PyAddress : Encodeable, Decodeable
    {
        public PyAddress()
        {
            type = AddrType.Invalid;
            typeID = 0;
            callID = 0;
            service = "";
            bcast_type = "";
        }

        public void Decode(PyObject from)
        {
            if (from.Type != PyObjectType.ObjectData)
            {
                throw new Exception($"Expected container to be of type ObjectData but got {from.Type}");
            }

            PyObjectData obj = from.As<PyObjectData>();

            if (obj.Name != "macho.MachoAddress")
            {
                throw new Exception($"Expected container to be of typeName 'macho.MachoAddress' but got {obj.Name}");
            }

            if (obj.Arguments.Type != PyObjectType.Tuple)
            {
                throw new Exception($"Expected arguments to be of type Tuple but got {obj.Arguments.Type}");
            }

            PyTuple args = obj.Arguments.As<PyTuple>();
            if (args.Items.Count < 3)
            {
                throw new Exception($"Expected arguments to have at least 3 items but got {args.Items.Count}");
            }

            if (args.Items[0].Type != PyObjectType.String)
            {
                throw new Exception($"Expected first argument to be of type String but got {args.Items[0].Type}");
            }

            PyString typei = args.Items[0].As<PyString>();

            switch (typei.Value)
            {
                case "A":
                    if (args.Items.Count != 3)
                    {
                        throw new Exception($"Expected arguments to have 3 items but got {args.Items.Count}");
                    }

                    DecodeService(args.Items[1]);
                    DecodeCallID(args.Items[2]);

                    type = AddrType.Any;
                    break;

                case "N":
                    if (args.Items.Count != 4)
                    {
                        throw new Exception($"Expected arguments to have 4 items but got {args.Items.Count}");
                    }
                    
                    DecodeTypeID(args.Items[1]);
                    DecodeService(args.Items[2]);
                    DecodeCallID(args.Items[3]);

                    type = AddrType.Node;

                    break;

                case "C":
                    if (args.Items.Count != 4)
                    {
                        throw new Exception($"Expected arguments to have 4 items but got {args.Items.Count}");
                    }

                    DecodeTypeID(args.Items[1]);
                    DecodeCallID(args.Items[2]);
                    DecodeService(args.Items[3]);

                    type = AddrType.Client;

                    break;

                case "B":
                    if (args.Items.Count != 4)
                    {
                        throw new Exception($"Expected arguments to have 4 items but got {args.Items.Count}");
                    }

                    type = AddrType.Broadcast;

                    if (args.Items[1].Type != PyObjectType.String)
                    {
                        throw new Exception($"Expected second argument to be of type String but got {args.Items[1].Type}");
                    }

                    if (args.Items[3].Type != PyObjectType.String)
                    {
                        throw new Exception($"Expected fourth argument to be of type String but got {args.Items[3].Type}");
                    }

                    PyString bid = args.Items[1].As<PyString>();
                    PyString idt = args.Items[3].As<PyString>();

                    service = bid.Value;
                    bcast_type = idt.Value;

                    break;

                default:
                    throw new Exception($"Unknown address type");
            }
        }

        private void DecodeTypeID(PyObject data)
        {
            if ((data.Type == PyObjectType.IntegerVar) || (data.Type == PyObjectType.Long) || (data.Type == PyObjectType.LongLong))
            {
                typeID = data.IntValue;
            }
            else if (data.Type == PyObjectType.None)
            {
                typeID = 0;
            }
            else
            {
                throw new Exception($"Expected typeID to be of type Integer, Long, LongLong or None but got {data.Type}");
            }
        }

        private void DecodeCallID(PyObject data)
        {
            if ((data.Type == PyObjectType.IntegerVar) || (data.Type == PyObjectType.Long) || (data.Type == PyObjectType.LongLong))
            {
                callID = data.IntValue;
            }
            else if (data.Type == PyObjectType.None)
            {
                callID = 0;
            }
            else
            {
                throw new Exception($"Expected callID to be of type Integer, Long, LongLong or None but got {data.Type}");
            }
        }

        private void DecodeService(PyObject data)
        {
            if (data.Type == PyObjectType.String)
            {
                service = data.As<PyString>().Value;
            }
            else if (data.Type == PyObjectType.None)
            {
                service = "";
            }
            else
            {
                throw new Exception($"Expected service to be of type String or None but got {data.Type}");
            }
        }

        public enum AddrType
        {
            Any = 'A',
            /*
             * [1] service
             * [2] callID
             */
            Node = 'N',
            /* [1] nodeID
             * [2] service
             * [3] callID
             */
            Client = 'C',
            /*
             * [1] clientID
             * [2] callID
             * [3] service
             */
            Broadcast = 'B',
            /*
             * [1] broadcastID
             * [2] narrowcast??
             * [3] idtype
             */
            Invalid = 'I' // Not real
        }

        /*  From client/script/common/net/machonet.py line: 3864(very old client source)
         *  Client = ('clientID', 'callID', 'service')
         *  Broadcast = ('broadcastID', 'narrowcast', 'idtype')
         *  Node = ('nodeID', 'service', 'callID')
         *  Any = ('service', 'callID')
         */

        public PyObject Encode()
        {
            PyTuple t = new PyTuple();

            switch (type)
            {
                case AddrType.Any:
                    t.Items.Add(new PyString("A"));

                    if (service == "")
                        t.Items.Add(new PyNone());
                    else
                        t.Items.Add(new PyString(service));

                    if (typeID == 0)
                        t.Items.Add(new PyNone());
                    else
                        t.Items.Add(new PyLongLong(typeID));
                    break;

                case AddrType.Node:
                    t.Items.Add(new PyString("N"));
                    t.Items.Add(new PyLongLong(typeID));

                    if (service == "")
                        t.Items.Add(new PyNone());
                    else
                        t.Items.Add(new PyString(service));

                    if (callID == 0)
                        t.Items.Add(new PyNone());
                    else
                        t.Items.Add(new PyLongLong(callID));

                    break;

                case AddrType.Client:
                    t.Items.Add(new PyString("C"));
                    t.Items.Add(new PyLongLong(typeID));
                    t.Items.Add(new PyLongLong(callID));

                    if (service == "")
                        t.Items.Add(new PyNone());
                    else
                        t.Items.Add(new PyString(service));

                    break;

                case AddrType.Broadcast:
                    t.Items.Add(new PyString("B"));

                    if (service == "")
                        t.Items.Add(new PyNone());
                    else
                        t.Items.Add(new PyString(service));

                    t.Items.Add(new PyList());
                    t.Items.Add(new PyString(bcast_type));
                    break;

                default:
                    break;
            }

            return new PyObjectData("macho.MachoAddress", t);
        }


        public AddrType type = AddrType.Invalid;
        public long typeID = 0; // NodeID, ClientID, etc
        public long callID = 0;

        public string service = ""; // broadcastID for a broadcast
        public string bcast_type = "";
    }
}
