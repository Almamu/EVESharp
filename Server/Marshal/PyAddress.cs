using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marshal
{
    // Ported from EVEmu: The EVE Online emulator
    // This class eases the process of reading the macho.MachoAddress PyObjectData
    // To create a easy reply

    public class PyAddress
    {
        public PyAddress()
        {
            type = AddrType.Invalid;
            typeID = 0;
            callID = 0;
            service = "";
            bcast_type = "";
        }

        public bool Decode(PyObject from)
        {
            if (from.Type != PyObjectType.ObjectData)
            {
                return false;
            }

            PyObjectData obj = from.As<PyObjectData>();

            if (obj.Name != "macho.MachoAddress")
            {
                return false;
            }

            if (obj.Arguments.Type != PyObjectType.Tuple)
            {
                return false;
            }

            PyTuple args = obj.Arguments.As<PyTuple>();
            if (args.Items.Count < 3)
            {
                return false;
            }

            if (args.Items[0].Type != PyObjectType.String)
            {
                return false;
            }

            PyString typei = args.Items[0].As<PyString>();

            switch (typei.Value)
            {
                case "A":
                    if (args.Items.Count != 3)
                    {
                        return false;
                    }

                    if (!DecodeService(args.Items[1]) || !DecodeCallID(args.Items[2]))
                    {
                        return false;
                    }

                    type = AddrType.Any;
                    break;

                case "N":
                    if (args.Items.Count != 4)
                    {
                        return false;
                    }

                    if (!DecodeTypeID(args.Items[1]) || !DecodeService(args.Items[2]) || !DecodeCallID(args.Items[3]))
                    {
                        return false;
                    }

                    type = AddrType.Node;

                    break;

                case "C":
                    if (args.Items.Count != 4)
                    {
                        return false;
                    }

                    if (!DecodeTypeID(args.Items[1]) || !DecodeCallID(args.Items[2]) || !DecodeService(args.Items[3]))
                    {
                        return false;
                    }

                    type = AddrType.Client;

                    break;

                case "B":
                    if (args.Items.Count != 4)
                    {
                        return false;
                    }

                    type = AddrType.Broadcast;

                    if (args.Items[1].Type != PyObjectType.String)
                    {
                        return false;
                    }

                    if (args.Items[3].Type != PyObjectType.String)
                    {
                        return false;
                    }

                    PyString bid = args.Items[1].As<PyString>();
                    PyString idt = args.Items[3].As<PyString>();

                    service = bid.Value;
                    bcast_type = idt.Value;

                    break;

                default:
                    return false;
            }

            return true;
        }

        private bool DecodeTypeID(PyObject data)
        {
            if (( data.Type == PyObjectType.IntegerVar) || ( data.Type == PyObjectType.Long) )
            {
                typeID = (ulong)data.As<PyInt>().Value;
            }
            else if (data.Type == PyObjectType.None)
            {
                typeID = 0;
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool DecodeCallID(PyObject data)
        {
            if ( (data.Type == PyObjectType.IntegerVar) || (data.Type == PyObjectType.Long ) )
            {
                callID = (ulong)data.IntValue;
            }
            else if (data.Type == PyObjectType.LongLong)
            {
                callID = (ulong)data.As<PyLongLong>().Value;
            }
            else if (data.Type == PyObjectType.None)
            {
                callID = 0;
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool DecodeService(PyObject data)
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
                return false;
            }

            return true;
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

        /*  From client/script/common/net/machonet.py line: 3864
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
                    t.Items.Add( new PyString( "A" ));

                    if( service == "" )
                        t.Items.Add( new PyNone() );
                    else
                        t.Items.Add( new PyString( service ) );

                    if( typeID == 0 )
                        t.Items.Add(new PyNone());
                    else
                        t.Items.Add(new PyLongLong( (long)typeID ) );
                    break;

                case AddrType.Node:
                    t.Items.Add( new PyString( "N" ) );
                    t.Items.Add( new PyLongLong( (long)typeID ) );

                    if( service == "" )
                        t.Items.Add( new PyNone() );
                    else
                        t.Items.Add( new PyString( service ) );

                    if( callID == 0 )
                        t.Items.Add( new PyNone() );
                    else
                        t.Items.Add( new PyLongLong( (long)callID ) );

                    break;

                case AddrType.Client:
                    t.Items.Add( new PyString( "C" ) );
                    t.Items.Add(new PyLongLong((long)typeID));
                    t.Items.Add(new PyLongLong((long)callID));

                    if( service == "" )
                        t.Items.Add( new PyNone() );
                    else
                        t.Items.Add( new PyString( service ) );

                    break;

                case AddrType.Broadcast:
                    t.Items.Add( new PyString( "B" ) );

                    if( service == "" )
                        t.Items.Add( new PyNone() );
                    else
                        t.Items.Add( new PyString( service ) );

                    t.Items.Add( new PyList() );
                    t.Items.Add( new PyString( bcast_type ) );
                    break;

                default:
                    break;
            }

            return new PyObjectData( "macho.MachoAddress", t );
        }


        public AddrType type = AddrType.Invalid;
        public ulong typeID = 0; // NodeID, ClientID, etc
        public ulong callID = 0;

        public string service = ""; // broadcastID for a broadcast
        public string bcast_type = "";
    }
}
