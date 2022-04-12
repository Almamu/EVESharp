namespace EVESharp.Orchestator.Models;

public class Node
{
    public string IP            { get; init; }
    public short  Port          { get; init; }
    public int    NodeID        { get; init; }
    public string Role          { get; init; }
    public long   LastHeartBeat { get; init; }
}