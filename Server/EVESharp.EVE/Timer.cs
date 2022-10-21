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
        this.mTimer           =  new Timer ((time - DateTime.UtcNow).TotalMilliseconds);
        this.mTimer.Elapsed   += this.OnTimerFired;
        this.mTimer.AutoReset =  false;
        this.mTimer.Enabled   =  true;
    }

    public Timer (TimeSpan interval, T state, Action <T> callback, ILogger logger)
    {
        this.Callback         =  callback;
        this.State            =  state;
        this.Log              =  logger;
        this.mTimer           =  new Timer (interval.TotalMilliseconds);
        this.mTimer.Elapsed   += this.OnTimerFired;
        this.mTimer.AutoReset =  true;
        this.mTimer.Enabled   =  true;
    }

    private void OnTimerFired (object sender, ElapsedEventArgs args)
    {
        try
        {
            // run the callback
            this.Callback (this.State);
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
    
    public void Dispose ()
    {
        this.mTimer?.Dispose ();
    }
}