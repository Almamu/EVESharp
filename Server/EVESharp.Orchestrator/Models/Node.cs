namespace EVESharp.Orchestrator.Models;

public class Node
{
    public string IP            { get; init; }
    public string Address       { get; init; }
    public int    Port          { get; init; }
    public int    NodeID        { get; init; }
    public string Role          { get; init; }
    public long   LastHeartBeat { get; init; }
    public double Load          { get; init; }
}