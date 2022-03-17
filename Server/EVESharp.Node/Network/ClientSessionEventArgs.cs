using EVESharp.EVE;
using EVESharp.EVE.Sessions;

namespace EVESharp.Node.Network
{
    public class ClientSessionEventArgs : ClientEventArgs
    {
        public Session Session { get; init; }
    }
}