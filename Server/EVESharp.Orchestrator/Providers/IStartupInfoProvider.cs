namespace EVESharp.Orchestrator.Providers;

public interface IStartupInfoProvider
{
    public DateTime Time { get; init; }
}