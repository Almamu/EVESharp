namespace EVESharp.EVE.Network.Messages;

public interface IMessageProcessor <T> where T : IMessage
{
    /// <summary>
    /// Adds a new message to the queue to be processed
    /// </summary>
    /// <param name="message"></param>
    void Enqueue (T message);

    /// <returns>The amount of messages pending</returns>
    int Count ();

    /// <summary>
    /// Stops this message processor and all it's threads
    /// </summary>
    void Stop ();
}