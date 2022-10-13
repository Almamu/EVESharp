using System;
using System.Timers;
using Serilog;

namespace EVESharp.EVE;

/// <summary>
/// Timer entry information
/// </summary>
public class Timer<T> : IDisposable
{
    private readonly Timer mTimer;
    
    public Timer (DateTime time, T state, Action<T> callback, ILogger logger)
    {
        this.Callback         =  callback;
        this.State            =  state;
        this.Log              =  logger;
        this.mTimer           =  new Timer ((time - DateTime.Now).TotalMilliseconds);
        this.mTimer.Elapsed   += this.OnTimerFired;
        this.mTimer.AutoReset =  false;
        this.mTimer.Enabled   =  true;
        this.ShouldStop       =  true;
    }

    public Timer (TimeSpan interval, T state, Action <T> callback, ILogger logger)
    {
        this.Callback         =  callback;
        this.State            =  state;
        this.Log              =  logger;
        this.mTimer           =  new Timer (interval.TotalMilliseconds);
        this.mTimer.Elapsed   += this.OnTimerFired;
        this.mTimer.AutoReset =  false;
        this.mTimer.Enabled   =  true;
        this.ShouldStop       =  false;
    }

    private void OnTimerFired (object sender, ElapsedEventArgs args)
    {
        try
        {
            // stop the timer
            if (this.ShouldStop)
                this.mTimer.Enabled = false;
            // run the callback
            this.Callback (this.State);
            // dispose of the timer
            if (this.ShouldStop)
                this.mTimer.Dispose ();    
        }
        catch (Exception e)
        {
            this.Log.Error ("Callback for timer threw an exception {e}", e);
            this.Log.Error (e.StackTrace);
        }
    }
    
    /// <summary>
    /// The method to call
    /// </summary>
    public Action <T> Callback { get; init; }
    
    /// <summary>
    /// The parameter to pass onto the <see cref="Callback"/>
    /// </summary>
    public T State { get; init; }

    /// <summary>
    /// Logger used to output logging messages on the timer
    /// </summary>
    private ILogger Log { get; }
    private bool ShouldStop { get; }
    public void Dispose ()
    {
        this.mTimer?.Dispose ();
    }
}