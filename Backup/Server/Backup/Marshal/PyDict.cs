using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Marshal
{

    public class PyDict : PyObject
    {
        public Dictionary<PyObject, PyObject> Dictionary { get; private set; }
        
        public PyDict()
            : base(PyObjectType.Dict)
        {
            Dictionary = new Dictionary<PyObject, PyObject>();
        }
        
        public PyDict(Dictionary<PyObject, PyObject> dict)
            : base(PyObjectType.Dict)
        {
            Dictionary = dict;
        }

        public PyObject Get(string key)
        {
            var keyObject =
                Dictionary.Keys.Where(k => k.Type == PyObjectType.String && (k as PyString).Value == key).FirstOrDefault();
            return keyObject == null ? null : Dictionary[keyObject];
        }

        public void Set(string key, PyObject value)
        {
            var keyObject = Dictionary.Count > 0 ? Dictionary.Keys.Where(k => k.Type == PyObjectType.String && (k as PyString).Value == key).FirstOrDefault() : null;
            if (keyObject != null)
                Dictionary[keyObject] = value;
            else
                Dictionary.Add(new PyString(key), value);
        }

        public bool Contains(string key)
        {
            return Dictionary.Keys.Any(k => k.Type == PyObjectType.String && (k as PyString).Value == key);
        }

        public override void Decode(Unmarshal context, MarshalOpcode op, BinaryReader source)
        {
            var entries = source.ReadSizeEx();
            Dictionary = new Dictionary<PyObject, PyObject>((int)entries);
            for (uint i = 0; i < entries; i++)
            {
                var value = context.ReadObject(source);
                var key = context.ReadObject(source);
                Dictionary.Add(key, value);
            }
        }

        protected override void EncodeInternal(BinaryWriter output)
        {
            output.WriteOpcode(MarshalOpcode.Dict);
            output.WriteSizeEx(Dictionary.Count);
            foreach (var pair in Dictionary)
            {
                pair.Value.Encode(output);
                pair.Key.Encode(output);
            }
        }

        public PyObject this[PyObject key]
        {
            get
            {
                return Dictionary[key];
            }
            set
            {
                Dictionary[key] = value;
            }
        }

        public override PyObject this[string key]
        {
            get
            {
                return Get(key);
            }
            set
            {
                Set(key, value);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder("<\n");
            foreach (var pair in Dictionary)
                sb.AppendLine("\t" + pair.Key + " " + pair.Value);
            sb.Append(">");
            return sb.ToString();
        }

    }

}