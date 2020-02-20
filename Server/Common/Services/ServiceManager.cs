
using System.Reflection;
using PythonTypes.Types.Primitives;

namespace Common.Services
{
    public class ServiceManager
    {
        public PyDataType ServiceCall(string service, string call, PyTuple data, object client)
        {
            MethodInfo method = GetType().GetMethod(service);

            if (method == null)
                throw new ServiceDoesNotExistsException(service);

            Service svc = (Service) (method.Invoke(this, null));

            method = svc.GetType().GetMethod(call);

            if (method == null)
                throw new ServiceDoesNotContainCallException(service, call);

            // relay the exception throw by the call
            try
            {
                return (PyDataType) (method.Invoke(svc, new object[] {data, client}));
            }
            catch (TargetInvocationException e)
            {
                // throw the InnerException if possible
                if (e.InnerException == null)
                    throw e;

                // throw the original exception
                throw e.InnerException;
            }
        }
    }
}