using System;

namespace EVESharp.EVE;

public interface ITimers
{
    Timer<T> EnqueueTimer<T> (DateTime dateTime, Action <T> callback, T parameter);
    Timer <T> EnqueueTimer <T> (TimeSpan timeSpan, Action <T> callback, T parameter);
}