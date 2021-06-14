using EVE;

namespace Node.Network
{
    public class ClientSessionEventArgs : ClientEventArgs
    {
        public Session Session { get; init; }
    }
}