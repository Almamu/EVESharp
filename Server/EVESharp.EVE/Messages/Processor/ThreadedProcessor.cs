using System;
using System.Collections.Generic;
using System.Threading;
using EVESharp.EVE.Messages.Queue;
using Serilog;

namespace EVESharp.EVE.Messages.Processor;

public class ThreadedProcessor<T> : IQueueProcessor<T> where T : IMessage
{
    public IMessageQueue <T> Queue { get; }
    /// <summary>
    /// The cancellation token all the threads will use to check for termination
    /// </summary>
    private CancellationTokenSource Token { get; set; }
    /// <summary>
    /// Number of threads to spawn to process the queue
    /// </summary>
    public int NumberOfThreads { get; } = 100;
    private ILogger Log { get; }
    /// <summary>
    /// List of running threads
    /// </summary>
    private readonly List <Thread> mThreads = new List <Thread> ();

    public ThreadedProcessor (IMessageQueue<T> queue, ILogger logger)
    {
        this.Queue = queue;
        this.Log   = logger;
    }

    /// <summary>
    /// Prepares the whole thread pool and starts them up
    /// </summary>
    private void InitializeThreads ()
    {
        this.Token = new CancellationTokenSource ();

        for (int i = 0; i < this.NumberOfThreads; i++)
        {
            Thread thread = new Thread (this.Run);
            thread.Start ();

            // add the thread to the list
            this.mThreads.Add (thread);
        }
    }

    /// <summary>
    /// Thread's body, gets messages and processes them
    /// </summary>
    private void Run ()
    {
        while (this.Token.IsCancellationRequested == false)
        {
            try
            {
                // wait for any data to be present in the queue
                if (this.Queue.TryTake (out T message, Timeout.Infinite, this.Token.Token) == true)
                    Queue.HandleMessage (message);
            }
            catch (Exception e)
            {
                Log.Error ("Exception handling message on MessageProcessor: {e}", e);
            }
        }
    }

    public void Start ()
    {
        if (this.mThreads.Count == 0)
            this.InitializeThreads ();
    }

    public void Stop ()
    {
        this.Token.Cancel ();
    }
}