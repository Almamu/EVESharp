using EVESharp.EVE.Messages.Queue;

namespace EVESharp.EVE.Messages.Processor;

public class SynchronousProcessor<T> : IQueueProcessor <T> where T : IMessage
{
    public IMessageQueue <T> Queue { get; }

    public SynchronousProcessor (IMessageQueue <T> queue)
    {
        this.Queue = queue;
    }
    
    public void Start ()
    {
        // these functions do nothing
    }

    public void Stop ()
    {
        // these functions do nothing
    }

    /// <summary>
    /// Processes the next message in the queue (if any)
    /// </summary>
    public void ProcessNextMessage ()
    {
        if (this.Queue.TryTake (out T message) == true)
            Queue.HandleMessage (message);
    }
}