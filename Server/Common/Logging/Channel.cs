using Renci.SshNet.Messages;

namespace Common.Logging
{
    public class Channel
    {
        public string Name { get; }
        public Logger Logger { get; }
        public bool Suppress { get; }
        
        public Channel(string name, Logger parent, bool suppress = false)
        {
            this.Name = name;
            this.Logger = parent;
            this.Suppress = suppress;
        }
        
        public void Debug(string message)
        {
            this.Logger.Write(MessageType.Debug, message, this);
        }

        public void Info(string message)
        {
            this.Logger.Write(MessageType.Info, message, this);
        }

        public void Trace(string message)
        {
            this.Logger.Write(MessageType.Trace, message, this);
        }

        public void Error(string message)
        {
            this.Logger.Write(MessageType.Error, message, this);
        }

        public void Fatal(string message)
        {
            this.Logger.Write(MessageType.Fatal, message, this);
        }

        public void Warning(string message)
        {
            this.Logger.Write(MessageType.Warning, message, this);
        }
    }
}