using System;
using EVESharp.EVE;
using EVESharp.EVE.Sessions;

namespace EVESharp.Node.Network
{
    public class ClientEventArgs : EventArgs
    {
        public Session Session { get; init; }
    }
}