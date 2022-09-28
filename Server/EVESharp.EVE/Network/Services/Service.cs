using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using EVESharp.EVE.Network.Services.Exceptions;
using EVESharp.EVE.Network.Services.Validators;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Network.Services;

public abstract class Service
{
    /// <summary>
    /// Indicates what pre-requirements the service has to be called by the client
    /// </summary>
    public abstract AccessLevel AccessLevel { get; }

    /// <summary>
    /// Returns the service name
    /// </summary>
    public string Name => this.GetType().Name;

    /// <summary>
    /// Searches for all the methods available with the given name
    /// </summary>
    /// <param name="methodName"></param>
    /// <returns>The list of methods found</returns>
    private bool FindSuitableMethod(string methodName, ServiceCall extra, out object[] parameters, out MethodInfo matchingMethod)
    {
        PyTuple      arguments      = extra.Payload;
        PyDictionary namedArguments = extra.NamedPayload;
        IEnumerable<MethodInfo> methods = this
                                          .GetType()
                                          .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                          .Where(x => x.Name == methodName)
                                          .OrderBy (x => x.GetParameters ().Length);

        matchingMethod = null;
        parameters     = null;
            
        foreach (MethodInfo method in methods)
        {
            ParameterInfo[] methodParameters = method.GetParameters();
                
            // ignore calls that have less parameters available that the ones provided
            // remember that last parameter is the service call information that isn't
            // provided by the call
            if (methodParameters.Length <= arguments.Count)
                continue;
            
            // this one should hold the real parameter count here
            parameters     = new object[methodParameters.Length];
            parameters[0]  = extra;

            bool match = true;

            for (int parameterIndex = 1, argumentIndex = 0; parameterIndex < methodParameters.Length; parameterIndex ++, argumentIndex ++)
            {
                if (argumentIndex >= arguments.Count)
                {
                    // check if the parameter is in the named payload and set it, otherwise resort to the default value
                    if (namedArguments.TryGetValue (methodParameters [parameterIndex].Name, out PyDataType value))
                    {
                        parameters [parameterIndex] = value;
                        match = true;
                        break;
                    }
                    if (methodParameters[parameterIndex].IsOptional == false)
                    {
                        match = false;
                        break;
                    }

                    // set the default value for the call
                    parameters[parameterIndex] = methodParameters[parameterIndex].DefaultValue;
                }
                else
                {
                    PyDataType element = arguments[argumentIndex];

                    if (element is null || methodParameters[parameterIndex].IsOptional)
                        parameters[parameterIndex] = null;
                    else if (methodParameters[parameterIndex].ParameterType == element.GetType() ||
                             methodParameters[parameterIndex].ParameterType == element.GetType().BaseType)
                        parameters[parameterIndex] = element;
                    else
                    {
                        match = false;
                        break;
                    }
                }
            }

            if (match)
            {
                matchingMethod = method;
                return true;
            }
        }

        return false;

    }

    /// <summary>
    /// Searches for the given <see cref="method"/> in the service and performs a call if the user is allowed
    /// </summary>
    /// <param name="method">The method to call</param>
    /// <param name="extraInformation">Extra information for the call</param>
    /// <returns>The returned data by the call (if any)</returns>
    public PyDataType ExecuteCall(string method, ServiceCall extraInformation)
    {
        if (this.FindSuitableMethod(method, extraInformation, out object[] parameters, out MethodInfo methodInfo) == false)
            throw new MissingCallException(this.Name, method);

        // ensure that the caller has the required roles
        List <CallValidator> requirements = this.GetType ().GetCustomAttributes <CallValidator> ().Concat (methodInfo.GetCustomAttributes <CallValidator> ()).ToList ();

        if (requirements.Any ())
        {
            foreach (CallValidator validator in requirements)
            {
                if (validator.Validate (extraInformation.Session) == false)
                {
                    if (validator.Exception is not null)
                        // throw that exception
                        throw (Exception) Activator.CreateInstance (validator.Exception, validator.ExceptionParameters);

                    return null;
                }
            }
        }

        try
        {
            // all checks completed, the caller has permissions to perform this call
            return (PyDataType) methodInfo.Invoke(this, parameters);
        }
        catch (TargetInvocationException e)
        {
            // throw the InnerException if possible
            // ExceptionDispatchInfo is used to preserve the stacktrace of the inner exception
            // so it gets rid of cryptic stacktraces that do not really point to the error
            if (e.InnerException is not null)
                ExceptionDispatchInfo.Throw(e.InnerException);
                
            // fallback, re-throw the original exception so at least there's some error information
            throw;
        }
    }
}