using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Common.Database;
using Common.Logging;
using Common.Services.Exceptions;
using Node.Configuration;
using PythonTypes.Types.Primitives;
using Node.Services;
using Service = Common.Services.Service;

namespace Node
{
    public class BoundServiceManager
    {
        public NodeContainer Container { get; }
        public Logger Logger { get; }
        
        private int mNextBoundID = 1;
        private Dictionary<int, BoundService> mBoundServices;
        private Channel Log { get; }

        public BoundServiceManager(NodeContainer container, Logger logger)
        {
            this.Logger = logger;
            this.Container = container;
            this.mBoundServices = new Dictionary<int, BoundService>();
            this.Log = this.Logger.CreateLogChannel("BoundService");
        }

        public int BoundService(BoundService service)
        {
            int boundID = this.mNextBoundID++;

            // add the service to the bound services map
            this.mBoundServices[boundID] = service;

            return boundID;
        }

        public string BuildBoundServiceString(int boundID)
        {
            return $"N={this.Container.NodeID}:{boundID}";
        }
        
        public PyDataType ServiceCall(int boundID, string call, PyTuple payload, object extraInformation)
        {
            BoundService serviceInstance = this.mBoundServices[boundID];
         
            Log.Trace($"Calling {serviceInstance.GetType().Name}::{call} on bound service {boundID}");
            
            if(serviceInstance == null)
                throw new ServiceDoesNotExistsException($"Bound Service {boundID}");

            List<MethodInfo> methods = serviceInstance
                .GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.Name == call)
                .ToList();
            
            if (methods.Any() == false)
                throw new ServiceDoesNotContainCallException($"(boundID {boundID}) {serviceInstance.GetType().Name}", call, payload);

            // relay the exception throw by the call
            try
            {
                foreach (MethodInfo method in methods)
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    object[] parameterList = new object[parameters.Length];

                    // set last parameters as these are the only ones that do not change
                    parameterList[^1] = extraInformation;

                    bool match = true;
                    
                    for (int i = 0; i < parameterList.Length - 1; i++)
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

                throw new ServiceDoesNotContainCallException($"(boundID {boundID}) {serviceInstance.GetType().Name}", call, payload);
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