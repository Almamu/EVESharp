using System;
using System.Collections.Generic;
using System.IO;

namespace Marshal
{

    public static class Marshal
    {
        public static byte[] Process(PyObject obj)
        {
            var ret = new MemoryStream(100);
            ret.WriteByte(Unmarshal.HeaderByte);
            // we have no support for save lists right now
            ret.WriteByte(0x00);
            ret.WriteByte(0x00);
            ret.WriteByte(0x00);
            ret.WriteByte(0x00);
            obj.Encode(new BinaryWriter(ret));
            return ret.ToArray();
        }

        public static PyTuple Tuple(params PyObject[] objs)
        {
            return new PyTuple(new List<PyObject>(objs));
        }

        public static PyDict Dict(params object[] objs)
        {
            if (objs.Length % 2 == 1)
                throw new ArgumentException("Expected pair arguments");

            var ret = new PyDict(new Dictionary<PyObject, PyObject>(objs.Length / 2));
            for (int i = 0; i < (objs.Length/2); i++)
            {
                var key = objs[i];
                var val = objs[i + 1];
                if (!(key is string))
                    throw new ArgumentException("Expected string");
                if (!(val is PyObject))
                    throw new ArgumentException("Expected PyObject");
                ret.Dictionary.Add(new PyString(key as string), val as PyObject);
            }
            return ret;
        }
    }

}