using System;
using System.Collections.Generic;
using System.Threading;
using Serilog;

namespace EVESharp.Node;

/// <summary>
/// Main timer manager used by the Node to do time-related stuff.
///
/// The timers used here are precise to the second, but might not fire up at the exact second
/// as it'll depend on the load on it. Used only for non-time sensitive stuff
/// </summary>
public class Timers
{
    private readonly List <Timer> mTimers = new List <Timer> ();
    private readonly Thread       mThread;
    private          ILogger      Log { get; }

    public Timers (ILogger logger)
    {
        Log          = logger;
        this.mThread = new Thread (this.Run);
    }

    /// <summary>
    /// Starts the thread for the TimerManager
    /// </summary>
    public void Start ()
    {
        this.mThread.Start ();
    }

    public Timer EnqueueTimer (long dateTime, Action <int> callback, int parameter)
    {
        lock (this.mTimers)
        {
            Timer timer = new Timer ()
            {
                DateTime          = dateTime,
                Callback          = callback,
                CallbackParameter = parameter
            };
            this.mTimers.Add (timer);
            this.mTimers.Sort ((x, y) => x.DateTime.CompareTo (y.DateTime));

            return timer;
        }
    }

    /// <summary>
    /// Removes the item timer that matches the given criteria
    /// </summary>
    public void DequeueTimer (Timer timer)
    {
        lock (this.mTimers)
        {
            this.mTimers.Remove (timer);
        }
    }

    /// <summary>
    /// Main body of the timer thread
    /// </summary>
    private void Run ()
    {
        Log.Information ("Timer thread started");

        try
        {
            while (true)
            {
                // wait 1 second between ticks to keep the cpu usage low here
                Thread.Sleep (1000);

                long currentDateTime = DateTime.UtcNow.ToFileTimeUtc ();

                lock (this.mTimers)
                {
                    foreach (Timer timer in this.mTimers)
                    {
                        // only iterate until we find an event that is not due to be fired
                        if (timer.DateTime > currentDateTime)
                            break;

                        Log.Debug ($"Firing callback for timer on itemID {timer.CallbackParameter}");

                        // TODO: MIGHT BE A GOOD IDEA TO HAVE THIS RUN ON A DIFFERENT THREAD?
                        try
                        {
                            timer.Callback?.Invoke (timer.CallbackParameter);
                        }
                        catch (Exception e)
                        {
                            Log.Error ("Callback for timer threw an exception {e}");
                        }
                        
                        timer.Handled = true;
                    }

                    // remove timers that have already been fired
                    this.mTimers.RemoveAll (x => x.Handled);
                }
            }
        }
        catch (Exception e)
        {
            Log.Fatal ($"Timer thread stopped: {e.Message}, timed events won't work from now on!!");
            Log.Fatal (e.StackTrace);
        }
    }
}