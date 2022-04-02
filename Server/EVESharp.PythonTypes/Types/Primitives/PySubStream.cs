using EVESharp.PythonTypes.Marshal;

namespace EVESharp.PythonTypes.Types.Primitives
{
    public class PySubStream : PyDataType
    {
        private bool mIsUnmarshaled = false;
        private PyDataType mCurrentStream = null;
        private PyDataType mOriginalStream = null;
        private byte[] mByteStream = null;

        public PyDataType Stream
        {
            get
            {
                if (this.mIsUnmarshaled == false)
                    this.mOriginalStream = this.mCurrentStream = Unmarshal.ReadFromByteArray(this.mByteStream);

                return this.mCurrentStream;
            }

            set
            {
                this.mIsUnmarshaled = true;
                this.mCurrentStream = value;
            }
        }

        public byte[] ByteStream
        {
            get
            {
                // check hash codes and types to ensure they're equal
                if (this.mByteStream is not null && (this.mIsUnmarshaled == false || this.mCurrentStream == this.mOriginalStream))
                    return this.mByteStream;

                // make sure the old and new value are the same so checks work fine
                this.mOriginalStream = this.mCurrentStream;
                
                // update the byte stream with the new value
                return this.mByteStream = Marshal.Marshal.ToByteArray(this.mCurrentStream);
            }
        }

        public PySubStream(byte[] from)
        {
            this.mIsUnmarshaled = false;
            this.mByteStream = from;
        }
        
        public PySubStream(PyDataType stream)
        {
            this.mIsUnmarshaled = true;
            this.mOriginalStream = this.mCurrentStream = stream;
        }

        public override int GetHashCode()
        {
            return (int) CRC32.Checksum(this.ByteStream) ^ 0x35415879;
        }
    }
}