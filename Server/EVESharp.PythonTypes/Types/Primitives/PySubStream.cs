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
                if (this.mIsUnmarshaled == false ||
                    (this.mCurrentStream?.GetHashCode() == this.mOriginalStream?.GetHashCode() && this.mByteStream is not null))
                    return this.mByteStream;

                this.mIsUnmarshaled = true;
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
            // TODO: PROPERLY REFINE THIS, RIGHT NOW THE HASHCODE WILL BE DIFFERENT BASED ON THE CURRENT DECODE STATUS
            if (this.mByteStream is not null)
                return this.mByteStream.GetHashCode();
            if (this.mCurrentStream is not null)
                return this.mCurrentStream.GetHashCode() * 2;
            if (this.mOriginalStream is not null)
                return this.mOriginalStream.GetHashCode() * 2;
            
            return 0;
        }
    }
}