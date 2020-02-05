namespace Common.Constants
{
    public class Network
    {
        /// <summary>
        /// The maximum memory reserved for any kind of I/O buffer in sockets
        /// </summary>
        public const int PACKET_BUFFER = 64 * 1024;
        
        /// <summary>
        /// Maximum allowed packet for any network operation (this is hardcoded on the client's code)
        /// </summary>
        public const int MAX_PACKET_SIZE = 10 * 1024 * 1024;
        
        /// <summary>
        /// The ID for the proxy node
        /// </summary>
        public const int PROXY_NODE_ID = 0xFFAA;
    }
}