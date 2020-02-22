
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
            MethodInfo method = serviceInstance.GetType().GetMethod(call);
            
            if(method == null)
                throw new ServiceDoesNotContainCallException(service, call);

            // relay the exception throw by the call
            try
            {
                ParameterInfo[] parameters = method.GetParameters();
                object[] parameterList = new object[parameters.Length];

                // set last parameters as these are the only ones that do not change
                parameterList[parameterList.Length - 1] = client;
                parameterList[parameterList.Length - 2] = namedPayload;

                for (int i = 0; i < parameterList.Length - 2; i++)
                {
                    if((i >= payload.Count || payload[i].GetType() != parameters[i].ParameterType) && parameters[i].IsOptional == false)
                        throw new CallArgumentsException(parameters[i].Name, parameters[i].ParameterType);

                    if (parameters[i].IsOptional == true && i >= payload.Count)
                        parameterList[i] = null;
                    else
                        parameterList[i] = payload[i];
                }
                
                // prepare the arguments for the function
                return (PyDataType) (method.Invoke(serviceInstance, parameterList));
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