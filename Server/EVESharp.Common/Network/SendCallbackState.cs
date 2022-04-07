namespace EVESharp.Common.Network;

public class SendCallbackState
{
    public byte [] Buffer { get; }
    public int     Sent   { get; set; }

    public SendCallbackState (byte [] buffer)
    {
        Buffer = buffer;
        Sent   = 0;
    }
}