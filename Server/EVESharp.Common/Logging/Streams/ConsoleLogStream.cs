using System;
using System.Collections.Generic;
using System.Threading;

namespace EVESharp.Common.Logging.Streams
{
    public class ConsoleLogStream : ILogStream
    {
        private Queue<StreamMessage> mQueue = new Queue<StreamMessage>();

        public void Write(MessageType messageType, string message, Channel channel)
        {
            StreamMessage entry = new StreamMessage(messageType, message, channel);

            lock(this.mQueue)
                this.mQueue.Enqueue(entry);
        }

        private ConsoleColor GetColorForMessageType(MessageType type)
        {
            switch (type)
            {
                case MessageType.Debug:
                    return ConsoleColor.Cyan;
                case MessageType.Error:
                    return ConsoleColor.Red;
                case MessageType.Warning:
                    return ConsoleColor.Yellow;
                case MessageType.Info:
                    return ConsoleColor.Green;
                case MessageType.Trace:
                    return ConsoleColor.Gray;
                case MessageType.Fatal:
                    return ConsoleColor.DarkRed;
                default:
                    return ConsoleColor.Gray;
            }
        }

        public void Flush()
        {
            Queue<StreamMessage> queue;
            
            lock (this.mQueue)
            {
                // if there is no message pending there is not an actual reason to flush the stream
                // so just release the semaphore and return
                if (this.mQueue.Count == 0)
                {
                    return;
                }

                // clone the queue so the services that need to write messages do not have to wait for the write to finish
                queue = this.mQueue;
                this.mQueue = new Queue<StreamMessage>();
            }

            while (queue.Count > 0)
            {
                StreamMessage entry = queue.Dequeue();

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write($"{entry.Time.DateTime.ToShortDateString()} {entry.Time.DateTime.ToShortTimeString()}");

                Console.ForegroundColor = this.GetColorForMessageType(entry.Type);
                Console.Write($" {entry.Type.ToString()[0]} ");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{entry.Channel.Name}: ");

                Console.ForegroundColor = this.GetColorForMessageType(entry.Type);
                Console.WriteLine(entry.Message);

                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
    }
}