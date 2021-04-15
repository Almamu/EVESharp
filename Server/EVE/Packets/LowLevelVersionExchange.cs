using Common;
using System;
using System.IO;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace EVE.Packets
{
    public class LowLevelVersionExchange
    {
        public int Birthday { get; init; }
        public int MachoVersion { get; init; }
        public int UserCount { get; set; }
        public double Version { get; init; }
        public int Build { get; init; }
        public string Codename { get; init; }
        public string Region { get; init; }
        public string NodeIdentifier { get; set; }
        public bool IsNode { get; set; } // 0-> Client, 1-> Node

        public static implicit operator LowLevelVersionExchange(PyDataType data)
        {
            if (data is not PyTuple exchange || exchange.Count != 6)
                throw new InvalidDataException("LowLevelVersionExchange must have 6 elements");

            LowLevelVersionExchange result = new LowLevelVersionExchange
            {
                Birthday = exchange[0] as PyInteger,
                MachoVersion = exchange[1] as PyInteger,
                Version = exchange[3] as PyDecimal,
                Build = exchange[4] as PyInteger,
                Codename = exchange[5] as PyString,
            };
            
            if (exchange[2] is PyString)
            {
                result.IsNode = true;
                result.NodeIdentifier = exchange[2] as PyString;
            }
            else
            {
                result.UserCount = exchange[2] as PyInteger;
            }
            
            if (result.Birthday != Game.BIRTHDAY)
                throw new InvalidDataException("Wrong birthday in LowLevelVersionExchange");
            if (result.Build != Game.BUILD)
                throw new InvalidDataException("Wrong build in LowLevelVersionExchange");
            if (result.Codename != Game.CODENAME + "@" + Game.REGION)
                throw new InvalidDataException("Wrong codename in LowLevelVersionExchange");
            if (result.MachoVersion != Game.MACHO_VERSION)
                throw new InvalidDataException("Wrong machoVersion in LowLevelVersionExchange");
            if (Math.Abs(result.Version - Game.VERSION) > 0.001)
                throw new InvalidDataException("Wrong version in LowLevelVersionExchange");
            if (result.IsNode == true && result.NodeIdentifier != "Node")
                throw new InvalidDataException("Wrong node string in LowLevelVersionExchange");

            return result;
        }

        public static implicit operator PyDataType(LowLevelVersionExchange exchange)
        {
            return new PyTuple(6)
            {
                [0] = exchange.Birthday,
                [1] = exchange.MachoVersion,
                [2] = (exchange.IsNode == true) ? exchange.NodeIdentifier : exchange.UserCount,
                [3] = exchange.Version,
                [4] = exchange.Build,
                [5] = exchange.Codename + "@" + exchange.Region
            };
        }
    }
}