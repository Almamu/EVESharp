using System;

namespace Common.Logging.Streams
{
    public class StreamMessage
    {
        public MessageType Type { get; }
        public string Message { get; }
        public Channel Channel { get; }
        public DateTimeOffset Time { get; }

        public StreamMessage(MessageType type, string message, Channel channel)
        {
            this.Type = type;
            this.Message = message;
            this.Channel = channel;
            this.Time = DateTime.UtcNow;
        }
    }
}