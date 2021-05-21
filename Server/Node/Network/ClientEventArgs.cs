using System;

namespace Node.Network
{
    public class ClientEventArgs : EventArgs
    {
        public Client Client { get; init; }
    }
}