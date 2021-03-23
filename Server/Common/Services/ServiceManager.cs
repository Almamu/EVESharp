
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
        public PyDataType ServiceCall(string service, string call, PyTuple arguments, object extraInformation)
        {
            object serviceObject = GetType().GetProperty(service)?.GetValue(this);
            
            if(serviceObject == null || serviceObject is IService == false)
                throw new ServiceDoesNotExistsException(service);

            IService serviceInstance = serviceObject as IService;
            List<MethodInfo> methods = serviceInstance
                .GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.Name == call)
                .ToList();
            
            if (methods.Any() == false)
                throw new ServiceDoesNotContainCallException(service, call, arguments);

            // relay the exception throw by the call
            try
            {
                foreach (MethodInfo method in methods)
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    
                    // ignore functions that do not have enough parameters in them
                    if (parameters.Length < (arguments.Count + 1))
                        continue;

                    object[] parameterList = new object[parameters.Length];
                    
                    // set last parameters as these are the only ones that do not change
                    parameterList[^1] = extraInformation;

                    bool match = true;
                    
                    for (int i = 0; i < parameterList.Length - 1; i++)
                    {
                        if (i >= arguments.Count)
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
                            PyDataType element = arguments[i];
                        
                            // check parameter types
                            if (element is null || parameters[i].IsOptional == true)
                                parameterList[i] = null;
                            else if (parameters[i].ParameterType == element.GetType() ||
                                     parameters[i].ParameterType == element.GetType().BaseType)
                                parameterList[i] = element;
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

                throw new ServiceDoesNotContainCallException(service, call, arguments);
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