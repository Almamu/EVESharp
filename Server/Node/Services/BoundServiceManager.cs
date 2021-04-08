using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Common.Logging;
using Common.Services.Exceptions;
using Node.Network;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Services
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
        private readonly Dictionary<int, BoundService> mBoundServices;
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
            lock (this.mBoundServices)
            {
                int boundID = this.mNextBoundID++;

                // add the service to the bound services map
                this.mBoundServices[boundID] = service;

                return boundID;
            }
        }

        /// <summary>
        /// Removes the given boundID service off the list
        /// </summary>
        /// <param name="boundID"></param>
        public void FreeBoundService(int boundID)
        {
            Log.Debug($"Freeing bound service {boundID}");
            
            lock (this.mBoundServices)
                this.mBoundServices.Remove(boundID);
        }
        
        /// <param name="boundID">The boundID to generate the string for</param>
        /// <returns>A string representation of the given boundID</returns>
        public string BuildBoundServiceString(int boundID)
        {
            return $"N={this.Container.NodeID}:{boundID}";
        }

        /// <summary>
        /// Notifies the bound service manager that the client disconnected
        /// This frees all the bound services this client has requested
        /// </summary>
        /// <param name="client">The client that disconnected</param>
        public void OnClientDisconnected(Client client)
        {
            List<int> boundServiceIDsToRemove = new List<int>();
            
            // search in all bound services and ensure the ones belonging to this client are free
            lock (this.mBoundServices)
            {
                foreach ((int boundID, BoundService service) in this.mBoundServices)
                {
                    // if the bound service belongs to this client
                    // add it to the removal list
                    if (service.Client == client)
                    {
                        // notify the service that we're freeing it
                        service.OnServiceFree();
                        // add it to the free'd list for removal off the list
                        boundServiceIDsToRemove.Add(boundID);                        
                    }
                }

                foreach (int key in boundServiceIDsToRemove)
                    this.FreeBoundService(key);
            }
        }
        
        /// <summary>
        /// Takes the given payload and searches in this service manager for the best service match to call the given method
        /// if possible
        /// </summary>
        /// <param name="boundID">The boundID to call at</param>
        /// <param name="call">The method to call</param>
        /// <param name="payload">Parameters for the method</param>
        /// <param name="callInformation">Any extra information for the method call</param>
        /// <returns>The result of the call</returns>
        /// <exception cref="ServiceDoesNotExistsException">If the boundID doesn't match any registered bound service</exception>
        /// <exception cref="ServiceDoesNotContainCallException">If the service was found but no matching call was found</exception>
        public PyDataType ServiceCall(int boundID, string call, PyTuple payload, CallInformation callInformation)
        {
            BoundService serviceInstance = this.mBoundServices[boundID];
         
            Log.Trace($"Calling {serviceInstance.GetType().Name}::{call} on bound service {boundID}");
            
            if(serviceInstance is null)
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
                    parameterList[^1] = callInformation;

                    bool match = true;
                    
                    for (int i = 0; i < parameterList.Length - 1; i++)
                    {
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
                            if (parameters[i].IsOptional == true || element is null)
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

                throw new ServiceDoesNotContainCallException($"(boundID {boundID}) {serviceInstance.GetType().Name}", call, payload);
            }
            catch (TargetInvocationException e)
            {
                // throw the InnerException if possible
                // ExceptionDispatchInfo is used to preserve the stacktrace of the inner exception
                // getting rid of cryptic stack traces that do not really tell much about the error
                if (e.InnerException is not null)
                    ExceptionDispatchInfo.Throw(e.InnerException);

                // if no internal exception was found re-throw the original exception
                throw;
            }
        }
    }
}