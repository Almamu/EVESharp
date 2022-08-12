using System.Collections.Concurrent;

namespace EVESharp.EVE.Messages.Queue;

public abstract class MessageQueue <T> : BlockingCollection<T>, IMessageQueue <T> where T : IMessage
{
    /// <summary>
    /// Adds a new message to the queue to be processed
    /// </summary>
    /// <param name="message"></param>
    public void Enqueue (T message)
    {
        this.Add (message);
    }

    /// <summary>
    /// Body of processing thread
    /// </summary>
    public abstract void HandleMessage (T message);
}