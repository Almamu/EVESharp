namespace EVESharp.Common.Network
{
    public class ReceiveCallbackState
    {
        public byte[] Buffer { get; set; }
        public int Received { get; set; }

        public ReceiveCallbackState(byte[] buffer)
        {
            this.Buffer = buffer;
            this.Received = 0;
        }
    }
}