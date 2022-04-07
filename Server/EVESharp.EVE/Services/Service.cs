using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using EVESharp.EVE.Services.Exceptions;
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
    public string Name => GetType().Name;

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
                                          .Where(x => x.Name == methodName);

        parameters     = new object[arguments.Count + 1];
        parameters[^1] = extra;
        matchingMethod = null;
            
        foreach (MethodInfo method in methods)
        {
            ParameterInfo[] methodParameters = method.GetParameters();
                
            // ignore calls that have less parameters available that the ones provided
            if (methodParameters.Length < parameters.Length)
                continue;

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

                    parameters[i] = null;
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
            
        // ensure that the caller has the required roles
        List<RequiredRole> attributes = methodInfo.GetCustomAttributes<RequiredRole>().ToList();

        if (attributes.Any() == true)
        {
            bool allowed = false;
                
            foreach (RequiredRole requirement in attributes)
            {
                if ((requirement.Role & extraInformation.Session.Role) == requirement.Role)
                {
                    // at least ONE of the roles match, so mark it as allowed and break the loop
                    allowed = true;
                    break;
                }
            }

            if (allowed == false)
                throw new UnauthorizedCallException<string>(Name, method, extraInformation.Session.Role);
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