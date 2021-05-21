using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Common.Logging;
using Common.Services.Exceptions;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Network
{
    /// <summary>
    /// Special service manager that handles Bound objects from the client
    ///
    /// These bound objects are usually stateful services that keep information about the player,
    /// location, items, etc, and is used as a way of managing the resources
    /// </summary>
    public class BoundServiceManager : Common.Services.ServiceManager
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
        public int BindService(BoundService service)
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
        /// Removes the bound service and unregisters it from the manager
        /// </summary>
        /// <param name="service">The service to unbind</param>
        public void UnbindService(BoundService service)
        {
            this.Log.Debug($"Unbinding service {service.BoundID}");
            
            // remove the service from the bound list
            this.mBoundServices.Remove(service.BoundID);
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
            // relay the exception throw by the call
            try
            {
                if (this.mBoundServices.TryGetValue(boundID, out BoundService serviceInstance) == false)
                    throw new Exception($"Unknown bound service {boundID}::{call}");
         
                Log.Trace($"Calling {serviceInstance.GetType().Name}::{call} on bound service {boundID}");
            
                if(serviceInstance is null)
                    throw new ServiceDoesNotExistsException($"Bound Service {boundID}");

                List<MethodInfo> methods = this.FindMethods(serviceInstance, $"(boundID {boundID}) {serviceInstance.GetType().Name}", call);

                if (FindSuitableMethod(methods, payload, callInformation, out object[] invokeParameters, out MethodInfo method) == false)
                    throw new ServiceDoesNotContainCallException($"(boundID {boundID}) {serviceInstance.GetType().Name}", call);

                return (PyDataType) method.Invoke(serviceInstance, invokeParameters);
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