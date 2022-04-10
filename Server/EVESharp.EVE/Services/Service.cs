using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using EVESharp.EVE.Services.Exceptions;
using EVESharp.EVE.Services.Validators;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Services;

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
        PyTuple arguments = extra.Payload;
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
            parameters[^1] = extra;

            bool match = true;

            for (int i = 0; i < methodParameters.Length - 1; i++)
            {
                if (i >= arguments.Count)
                {
                    if (methodParameters[i].IsOptional == false)
                    {
                        match = false;
                        break;
                    }

                    // set the default value for the call
                    parameters[i] = methodParameters[i].DefaultValue;
                }
                else
                {
                    PyDataType element = arguments[i];

                    if (element is null || methodParameters[i].IsOptional == true)
                        parameters[i] = null;
                    else if (methodParameters[i].ParameterType == element.GetType() ||
                             methodParameters[i].ParameterType == element.GetType().BaseType)
                        parameters[i] = element;
                    else
                    {
                        match = false;
                        break;
                    }
                }
            }

            if (match == true)
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
            throw new MissingCallException(Name, method);
        
        // TODO: SUPPORT NAMED PAYLOAD ARGUMENTS AS FUNCTION ARGUMENTS
        
        // ensure that the caller has the required roles
        List <CallValidator> requirements = this.GetType ().GetCustomAttributes <CallValidator> ().Concat (methodInfo.GetCustomAttributes <CallValidator> ()).ToList ();

        if (requirements.Any () == true)
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