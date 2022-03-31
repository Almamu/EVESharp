using Serilog;

namespace EVESharp.Common.Logging;

public static class LoggingExtensions
{
    public const string HIDDEN_PROPERTY_NAME = "HiddenByDefault";
    
    /// <summary>
    /// Custom ForContext that adds a name to the logger that is different from the class name
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="name"></param>
    /// <param name="hiddenByDefault">Indicates if the logging context should be hidden by default</param>
    /// <typeparam name="TSource"></typeparam>
    /// <returns></returns>
    public static ILogger ForContext<TSource>(this ILogger logger, string name, bool hiddenByDefault = false)
    {
        ILogger newLogger = logger
            .ForContext<TSource>()
            .ForContext("Name", name);
            
        if (hiddenByDefault == true)
            newLogger = newLogger.ForContext(HIDDEN_PROPERTY_NAME, hiddenByDefault);

        return newLogger;
    }
}