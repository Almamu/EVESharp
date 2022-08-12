using EVESharp.EVE.Messages.Queue;

namespace EVESharp.EVE.Messages.Processor;

public interface IQueueProcessor<T> where T : IMessage
{
    /// <summary>
    /// The message queue this processor handles
    /// </summary>
    public IMessageQueue <T> Queue { get; }

    /// <summary>
    /// Starts the queue processor
    /// </summary>
    void Start ();

    /// <summary>
    /// Stops this message processor and all it's threads
    /// </summary>
    void Stop ();
}