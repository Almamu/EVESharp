
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using PythonTypes.Types.Primitives;
using Common.Services.Exceptions;
using PythonTypes.Types.Collections;

namespace Common.Services
{
    public class ServiceManager
    {
        protected IService FindService(string service)
        {
            object serviceObject = GetType().GetProperty(service)?.GetValue(this);

            if (serviceObject is not IService instance)
                throw new ServiceDoesNotExistsException(service);
            
            return instance;
        }
        
        protected List<MethodInfo> FindMethods(IService service, string serviceName, string method)
        {
            List<MethodInfo> methods = service
                .GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.Name == method)
                .ToList();
            
            if (methods.Count == 0)
                throw new ServiceDoesNotContainCallException(serviceName, method);
            
            return methods;
        }

        protected bool FindSuitableMethod(List<MethodInfo> methods, PyTuple arguments, object extraInformation, out object[] parameters, out MethodInfo matchingMethod)
        {
            parameters = new object[arguments.Count + 1];
            parameters[^1] = extraInformation;
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
        
        public PyDataType ServiceCall(string service, string call, PyTuple arguments, object extraInformation)
        {
            // relay the exception throw by the call
            try
            {
                IService serviceInstance = this.FindService(service);
                List<MethodInfo> methods = this.FindMethods(serviceInstance, service, call);
            
                if (FindSuitableMethod(methods, arguments, extraInformation, out object[] invokeParameters, out MethodInfo method) == false)
                    throw new ServiceDoesNotContainCallException(service, call);

                return (PyDataType) method.Invoke(serviceInstance, invokeParameters);
            }
            catch (TargetInvocationException e)
            {
                // throw the InnerException if possible
                // ExceptionDispatchInfo is used to preserve the stacktrace of the inner exception
                // getting rid of cryptic stack traces that do not really tell much about the error
                if (e.InnerException != null)
                    ExceptionDispatchInfo.Throw(e.InnerException);

                // if no internal exception was found re-throw the original exception
                throw;
            }
        }
    }
}