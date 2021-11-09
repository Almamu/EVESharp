using EVESharp.EVE;

namespace EVESharp.Node.Network
{
    public class ClientSessionEventArgs : ClientEventArgs
    {
        public Session Session { get; init; }
    }
}