using System.IO;
using PythonTypes.Types.Primitives;

namespace Common.Packets
{
    public class LowLevelVersionExchange
    {
        public int birthday = 0;
        public int machoVersion = 0;
        public int usercount = 0;
        public double version = 0.0;
        public int build = 0;
        public string codename = "";
        public string region = "";
        public string nodeIdentifier = "";
        public bool isNode = false; // 0-> Client, 1-> Node

        public static implicit operator LowLevelVersionExchange(PyDataType data)
        {
            PyTuple exchange = data as PyTuple;

            if (exchange.Count != 6)
                throw new InvalidDataException("LowLevelVersionExchange must have 6 elements");

            LowLevelVersionExchange result = new LowLevelVersionExchange();

            result.birthday = exchange[0] as PyInteger;
            result.machoVersion = exchange[1] as PyInteger;

            if (exchange[2] is PyString)
            {
                result.isNode = true;
                result.nodeIdentifier = exchange[2] as PyString;
            }
            else
            {
                result.usercount = exchange[2] as PyInteger;
            }

            result.version = exchange[3] as PyDecimal;
            result.build = exchange[4] as PyInteger;
            result.codename = exchange[5] as PyString;

            return result;
        }

        public static implicit operator PyDataType(LowLevelVersionExchange exchange)
        {
            return new PyTuple(new PyDataType[]
            {
                exchange.birthday, exchange.machoVersion,
                (exchange.isNode == true) ? (PyDataType) exchange.nodeIdentifier : exchange.usercount, exchange.version,
                exchange.build, exchange.codename + "@" + exchange.region
            });
        }
    }
}