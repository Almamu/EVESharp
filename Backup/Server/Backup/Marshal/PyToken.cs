using System.IO;
using System.Text;

namespace Marshal
{

    public class PyToken : PyObject
    {
        public byte[] RawToken { get; set; }
        public string Token { get; set; }

        public PyToken()
            : base(PyObjectType.Token)
        {
            
        }

        public PyToken(string token)
            : base(PyObjectType.Token)
        {
            Token = token;
        }

        public override void Decode(Unmarshal context, MarshalOpcode op, BinaryReader source)
        {
            byte len = source.ReadByte();
            RawToken = source.ReadBytes(len);
            Token = Encoding.ASCII.GetString(RawToken);
        }

        protected override void EncodeInternal(BinaryWriter output)
        {
            output.WriteOpcode(MarshalOpcode.Token);
            if (RawToken != null)
            {
                output.Write((byte)RawToken.Length);
                output.Write(RawToken);
            }
            else if (Token != null)
            {
                output.Write((byte)Token.Length);
                output.Write(Encoding.ASCII.GetBytes(Token));
            }
            else
                throw new InvalidDataException("Fill either RawToken or Token with data for encoding");
        }
    }

}