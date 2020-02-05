namespace Common.Network
{
    public class SendCallbackState
    {
        public byte[] Buffer { get; set; }
        public int Sent { get; set; }

        public SendCallbackState(byte[] buffer)
        {
            this.Buffer = buffer;
            this.Sent = 0;
        }
    }
}