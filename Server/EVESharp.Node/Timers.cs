using System;
using EVESharp.EVE;
using Serilog;

namespace EVESharp.Node;

/// <summary>
/// Main timer manager used by the Node to do time-related stuff.
///
/// The timers used here are precise to the second, but might not fire up at the exact second
/// as it'll depend on the load on it. Used only for non-time sensitive stuff
/// </summary>
public class Timers : ITimers
{
    private ILogger Log { get; }

    public Timers (ILogger logger)
    {
        Log = logger;
    }

    public Timer<T> EnqueueTimer<T> (DateTime dateTime, Action <T> callback, T parameter)
    {
        return new Timer<T> (dateTime, parameter, callback, this.Log);
    }

    public Timer <T> EnqueueTimer <T> (TimeSpan timeSpan, Action <T> callback, T parameter)
    {
        return new Timer <T> (timeSpan, parameter, callback, this.Log);
    }
}