using System.Threading;
using EVESharp.EVE.Messages.Processor;

namespace EVESharp.EVE.Messages.Queue;

public interface IMessageQueue <T> where T : IMessage
{
    /// <summary>
    /// Adds a new message to the queue to be processed
    /// </summary>
    /// <param name="message"></param>
    void Enqueue (T message);

    /// <returns>The amount of messages pending</returns>
    public int Count { get; }

    /// <summary>
    /// Body of processing thread
    /// </summary>
    void HandleMessage (T message);

    /// <summary>
    /// Attempts to remove an item rom the que queue
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryTake (out T value);
    
    /// <summary>
    /// Attempts to remove an item from the queue
    /// </summary>
    /// <param name="item"></param>
    /// <param name="millisecondsTimeout"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    bool TryTake (out T item, int millisecondsTimeout, CancellationToken cancellationToken);
}