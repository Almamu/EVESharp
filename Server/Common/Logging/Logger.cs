using System;
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
        private List<LogStream> mStreams = new List<LogStream>();
        private Dictionary<string, Channel> mChannels = new Dictionary<string, Channel>();

        public Logger()
        {
        }
        
        public void AddLogStream(LogStream newStream)
        {
            this.mStreams.Add(newStream);
        }

        public Channel CreateLogChannel(string name)
        {
            if (this.mChannels.ContainsKey(name) == true)
            {
                return this.mChannels[name];
            }
            
            Channel channel = new Channel(name, this);

            this.mChannels.Add(name, channel);
            
            return channel;
        }

        public void Write(MessageType messageType, string message, Channel channel = null)
        {
            // iterate all the log streams and queue the messages
            foreach(LogStream stream in this.mStreams)
            {
                stream.Write(messageType, message, channel);
            }
        }

        public void Flush()
        {
            foreach (LogStream stream in this.mStreams)
            {
                stream.Flush();
            }
        }
    }
}