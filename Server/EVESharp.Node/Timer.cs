using System;

namespace EVESharp.Node;

/// <summary>
/// Timer entry information
/// </summary>
public class Timer
{
    /// <summary>
    /// The method to call
    /// </summary>
    public Action <int> Callback { get; init; }
    /// <summary>
    /// The parameter to pass onto the <see cref="Callback"/>
    /// </summary>
    public int CallbackParameter { get; init; }
    /// <summary>
    /// The timestamp when the timer should be fired
    /// </summary>
    public long DateTime { get; init; }
    /// <summary>
    /// Indicates if this timer was already handled or not
    /// </summary>
    public bool Handled { get;  set; }
}