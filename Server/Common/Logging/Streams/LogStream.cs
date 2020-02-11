namespace Common.Logging.Streams
{
    public interface LogStream
    {
        void Write(MessageType messageType, string message, Channel channel);
        void Flush();
    }
}