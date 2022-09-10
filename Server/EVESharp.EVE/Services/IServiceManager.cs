using EVESharp.Types;

namespace EVESharp.EVE.Services;

public interface IServiceManager<in T>
{
    /// <summary>
    /// Resolves a service and handles the call to it
    /// </summary>
    /// <param name="service">The service where the method is</param>
    /// <param name="method">The method to call</param>
    /// <param name="call">Information about the call to perform</param>
    /// <returns>The return from the service call</returns>
    public PyDataType ServiceCall(T service, string method, ServiceCall call);
}