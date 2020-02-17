using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PythonTypes;
using PythonTypes.Types.Primitives;

namespace Common.Packets
{
    public class SessionChangeNotification
    {
        public int clueless = 0;
        public PyDictionary changes = new PyDictionary();
        public PyList nodesOfInterest = new PyList();

        public static implicit operator PyTuple(SessionChangeNotification notification)
        {
            return new PyTuple( new PyDataType[]
                {
                    new PyTuple(new PyDataType[] { notification.clueless, notification.changes }),
                    notification.nodesOfInterest
                }
            );
        }
    }
}
