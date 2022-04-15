namespace EVESharp.Orchestator.Providers;

public interface IStartupInfoProvider
{
    public DateTime Time { get; init; }
}