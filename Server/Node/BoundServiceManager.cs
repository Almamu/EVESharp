using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Common.Logging;
using Common.Services.Exceptions;
using Node.Services;
using PythonTypes.Types.Primitives;

namespace Node
{
    /// <summary>
    /// Special service manager that handles Bound objects from the client
    ///
    /// These bound objects are usually stateful services that keep information about the player,
    /// location, items, etc, and is used as a way of managing the resources
    ///
    /// TODO: IT MIGHT BE A GOOD IDEA TO SUPPORT TIMEOUTS FOR THESE OBJECTS
    /// </summary>
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

        /// <summary>
        /// Registers the given bound service into this service manager
        /// </summary>
        /// <param name="service">The bound service to register</param>
        /// <returns>The boundID of this service</returns>
        public int BoundService(BoundService service)
        {
            int boundID = this.mNextBoundID++;

            // add the service to the bound services map
            this.mBoundServices[boundID] = service;

            return boundID;
        }

        /// <summary>
        /// Removes the given boundID service off the list
        /// </summary>
        /// <param name="boundID"></param>
        public void FreeBoundService(int boundID)
        {
            Log.Debug($"Freeing bound service {boundID}");
            this.mBoundServices.Remove(boundID);
        }
        
        /// <param name="boundID">The boundID to generate the string for</param>
        /// <returns>A string representation of the given boundID</returns>
        public string BuildBoundServiceString(int boundID)
        {
            return $"N={this.Container.NodeID}:{boundID}";
        }
        
        /// <summary>
        /// Takes the given payload and searches in this service manager for the best service match to call the given method
        /// if possible
        /// </summary>
        /// <param name="boundID">The boundID to call at</param>
        /// <param name="call">The method to call</param>
        /// <param name="payload">Parameters for the method</param>
        /// <param name="extraInformation">Any extra information for the method call</param>
        /// <returns>The result of the call</returns>
        /// <exception cref="ServiceDoesNotExistsException">If the boundID doesn't match any registered bound service</exception>
        /// <exception cref="ServiceDoesNotContainCallException">If the service was found but no matching call was found</exception>
        public PyDataType ServiceCall(int boundID, string call, PyTuple payload, object extraInformation)
        {
            BoundService serviceInstance = this.mBoundServices[boundID];
         
            Log.Trace($"Calling {serviceInstance.GetType().Name}::{call} on bound service {boundID}");
            
            if(serviceInstance == null)
                throw new ServiceDoesNotExistsException($"Bound Service {boundID}");

            // ensure that only public methods that are part of the instance can be called
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

                    // ignore functions that do not have enough parameters in them
                    if (parameters.Length < (payload.Count + 1))
                        continue;
                    
                    object[] parameterList = new object[parameters.Length];

                    // set last parameters as these are the only ones that do not change
                    parameterList[^1] = extraInformation;

                    bool match = true;
                    
                    for (int i = 0; i < parameterList.Length - 1; i++)
                    {
                        // ensure the parameter count list matches
                        // search for a method that has enough parameters to handle this call
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