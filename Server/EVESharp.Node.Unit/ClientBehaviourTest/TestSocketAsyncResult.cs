using System;
using System.Threading;

namespace EVESharp.Node.Unit.ClientBehaviourTest;

public class TestSocketAsyncResult : IAsyncResult
{
    public object AsyncState { get; init; }
    public WaitHandle AsyncWaitHandle        { get; }
    public bool       CompletedSynchronously { get; }
    public bool       IsCompleted            { get; }
}