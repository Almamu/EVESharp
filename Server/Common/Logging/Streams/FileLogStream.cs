using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Common.Logging.Streams
{
    public class FileLogStream : LogStream
    {
        private readonly Configuration.FileLog mConfiguration = null;
        private Queue<StreamMessage> mQueue = new Queue<StreamMessage>();
        private readonly Semaphore mSemaphore = new Semaphore(1, 1);
        private readonly FileStream mFile = null;
        private readonly StreamWriter mWriter = null;

        public FileLogStream(Configuration.FileLog configuration)
        {
            this.mConfiguration = configuration;

            // create directory and log file
            if (Directory.Exists(this.mConfiguration.Directory) == false)
                Directory.CreateDirectory(this.mConfiguration.Directory);

            this.mFile = File.OpenWrite(Path.Join(this.mConfiguration.Directory, this.mConfiguration.LogFile));
            this.mWriter = new StreamWriter(this.mFile);
        }

        public void Write(MessageType messageType, string message, Channel channel)
        {
            StreamMessage entry = new StreamMessage(messageType, message, channel);

            this.mSemaphore.WaitOne();
            this.mQueue.Enqueue(entry);
            this.mSemaphore.Release();
        }

        public void Flush()
        {
            this.mSemaphore.WaitOne();

            // if there is no message pending there is not an actual reason to flush the stream
            // so just release the semaphore and return
            if (this.mQueue.Count == 0)
            {
                this.mSemaphore.Release();
                return;
            }

            // clone the queue so the services that need to write messages do not have to wait for the write to finish
            Queue<StreamMessage> queue = this.mQueue;
            this.mQueue = new Queue<StreamMessage>();

            this.mSemaphore.Release();

            while (queue.Count > 0)
            {
                StreamMessage entry = queue.Dequeue();

                this.mWriter.Write(
                    $"{entry.Time.DateTime.ToShortDateString()} {entry.Time.DateTime.ToShortTimeString()} {entry.Type.ToString()[0]} " +
                    $"{entry.Channel.Name}: {entry.Message}{Environment.NewLine}"
                );
            }

            this.mFile.Flush();
        }
    }
}