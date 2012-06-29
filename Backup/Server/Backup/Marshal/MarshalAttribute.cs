using System;

namespace Marshal
{
    
    [AttributeUsage(AttributeTargets.Class)]
    public class MarshalOptions : Attribute
    {
        public bool Checksum { get; set; }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class MarshalAs : Attribute
    {
        public MarshalOpcode Opcode { get; private set; }

        public MarshalAs(MarshalOpcode opcode)
        {
            Opcode = opcode;
        }
    }

}