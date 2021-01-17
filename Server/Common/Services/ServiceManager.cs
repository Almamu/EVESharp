
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using PythonTypes.Types.Primitives;
using Common.Services.Exceptions;

namespace Common.Services
{
    public class ServiceManager
    {
        public PyDataType ServiceCall(string service, string call, PyTuple payload, PyDictionary namedPayload, object client)
        {
            object serviceObject = GetType().GetProperty(service)?.GetValue(this);
            
            if(serviceObject == null || serviceObject is Service == false)
                throw new ServiceDoesNotExistsException(service);

            Service serviceInstance = serviceObject as Service;
            List<MethodInfo> methods = serviceInstance
                .GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.Name == call)
                .ToList();
            
            if (methods.Any() == false)
                throw new ServiceDoesNotContainCallException(service, call, payload);

            // relay the exception throw by the call
            try
            {
                foreach (MethodInfo method in methods)
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    object[] parameterList = new object[parameters.Length];

                    // set last parameters as these are the only ones that do not change
                    parameterList[^1] = client;
                    parameterList[^2] = namedPayload;

                    bool match = true;
                    
                    for (int i = 0; i < parameterList.Length - 2; i++)
                    {
                        if (i >= payload.Count)
                        {
                            if (parameters[i].IsOptional == false)
                            {
                                match = false;
                                break;                                
                            }

                            parameterList[i] = null;
                        }
                        else
                        {
                            PyDataType element = payload[i];
                        
                            // check parameter types
                            if (parameters[i].ParameterType == element.GetType() ||
                                parameters[i].ParameterType == element.GetType().BaseType)
                                parameterList[i] = element;
                            else if (parameters[i].IsOptional == true || element is PyNone)
                                parameterList[i] = null;
                            else
                            {
                                match = false;
                                break;
                            }
                        }
                    }
                
                    if (match)
                        // prepare the arguments for the function
                        return (PyDataType) (method.Invoke(serviceInstance, parameterList));
                }

                throw new ServiceDoesNotContainCallException(service, call, payload);
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