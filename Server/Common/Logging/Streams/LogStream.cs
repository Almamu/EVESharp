namespace Common.Logging.Streams
{
    public interface ILogStream
    {
        void Write(MessageType messageType, string message, Channel channel);
        void Flush();
    }
}