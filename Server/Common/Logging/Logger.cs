using System.Collections.Generic;
using Common.Logging.Streams;

namespace Common.Logging
{
    public enum MessageType
    {
        Debug = 0,
        Info = 1,
        Trace = 2,
        Error = 3,
        Fatal = 4,
        Warning = 5
    }

    public class Logger
    {
        private readonly List<LogStream> mStreams = new List<LogStream>();
        private readonly Dictionary<string, Channel> mChannels = new Dictionary<string, Channel>();
        private Configuration.Logging mConfiguration = null;

        public Logger(Configuration.Logging configuration)
        {
            this.mConfiguration = configuration;
        }

        public void AddLogStream(LogStream newStream)
        {
            this.mStreams.Add(newStream);
        }

        public Channel CreateLogChannel(string name, bool suppress = false)
        {
            lock (this.mChannels)
            {
                if (this.mChannels.ContainsKey(name) == true)
                    return this.mChannels[name];

                Channel channel = new Channel(name, this, suppress);

                this.mChannels.Add(name, channel);

                return channel;
            }
        }

        public void Write(MessageType messageType, string message, Channel channel)
        {
            // supress message if required
            if (channel.Suppress == true && this.mConfiguration.EnableChannels.Contains(channel.Name) == false)
                return;

            // iterate all the log streams and queue the messages
            foreach (LogStream stream in this.mStreams)
                stream.Write(messageType, message, channel);
        }

        public void Flush()
        {
            foreach (LogStream stream in this.mStreams)
                stream.Flush();
        }
    }
}