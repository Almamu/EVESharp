﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Serilog;

namespace EVESharp.Common.Network.Messages;

public abstract class MessageProcessor<T> where T : IMessage
{
    public int NumberOfThreads { get; init; }
    
    /// <summary>
    /// Messages to be processed
    /// </summary>
    private readonly BlockingCollection<T> mMessages = new BlockingCollection<T>();
    
    /// <summary>
    /// List of running threads
    /// </summary>
    private readonly List<Thread> mThreads = new List<Thread>();
    
    /// <summary>
    /// Logger used by this message processor
    /// </summary>
    protected ILogger Log { get; init; }

    public MessageProcessor(ILogger logger, int numberOfThreads)
    {
        this.NumberOfThreads = numberOfThreads;
        this.Log = logger;
        
        this.InitializeThreads();
    }

    /// <summary>
    /// Adds a new message to the queue to be processed
    /// </summary>
    /// <param name="message"></param>
    public void Enqueue(T message)
    {
        this.mMessages.Add(message);
    }

    /// <returns>The amount of messages pending</returns>
    public int Count()
    {
        return this.mMessages.Count;
    }

    /// <summary>
    /// Prepares the whole thread pool and starts them up
    /// </summary>
    private void InitializeThreads()
    {
        for (int i = 0; i < this.NumberOfThreads; i++)
        {
            Thread thread = new Thread(Run);
            thread.Start();
            
            // add the thread to the list
            this.mThreads.Add(thread);
        }
    }

    /// <summary>
    /// Thread's body, gets messages and processes them
    /// </summary>
    private void Run()
    {
        try
        {
            while (true)
            {
                try
                {
                    // wait for one second for there to be any data
                    if (this.mMessages.TryTake(out T message, 1000) == true)
                        this.HandleMessage(message);
                }
                catch (Exception e)
                {
                    Log.Error("Exception handling message on MessageProcessor: {e}", e);
                }
            }
        }
        catch (ThreadInterruptedException e)
        {
            // expected situation, let the run method end
        }
    }

    /// <summary>
    /// Stops this message processor and all it's threads
    /// </summary>
    public void Stop()
    {
        foreach (Thread thread in this.mThreads)
            thread.Interrupt();
    }

    /// <summary>
    /// Body of processing thread
    /// </summary>
    protected abstract void HandleMessage(T message);
}