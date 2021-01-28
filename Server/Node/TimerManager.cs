using System;
using System.Collections.Generic;
using System.Threading;
using Common.Logging;

namespace Node
{
    /// <summary>
    /// Main timer manager used by the Node to do time-related stuff.
    ///
    /// The timers used here are precise to the second, but might not fire up at the exact second
    /// as it'll depend on the load on it. Used only for non-time sensitive stuff
    /// </summary>
    public class TimerManager
    {
        private Thread mThread = null;
        private Channel Log = null;
        private List<Timer> mItemTimers = new List<Timer>();
        private List<Timer> mCallTimers = new List<Timer>();
        
        public TimerManager(Logger logger)
        {
            this.Log = logger.CreateLogChannel("TimerManager");
            this.mThread = new Thread(Run);
        }

        /// <summary>
        /// Starts the thread for the TimerManager
        /// </summary>
        public void Start()
        {
            this.mThread.Start();
        }
        
        /// <summary>
        /// Adds a new timed event related to an item
        /// </summary>
        /// <param name="dateTime">The timestamp when the event should fire up</param>
        /// <param name="callback">What function to call</param>
        /// <param name="itemID">The related itemID</param>
        public void EnqueueItemTimer(long dateTime, Action<int> callback, int itemID)
        {
            lock (this.mItemTimers)
            {
                this.mItemTimers.Add(new Timer () { DateTime = dateTime, Callback = callback, CallbackParameter = itemID });
                // reorder the list
                this.mItemTimers.Sort((x, y) => x.DateTime.CompareTo(y.DateTime));
            }
        }

        /// <summary>
        /// Adds a new timed call
        /// </summary>
        /// <param name="dateTime">The timestamp when the event should fire up</param>
        /// <param name="callback">What function to call</param>
        /// <param name="callID">The callID it's related to</param>
        public void EnqueueCallTimer(long dateTime, Action<int> callback, int callID)
        {
            lock (this.mCallTimers)
            {
                this.mCallTimers.Add(new Timer () { DateTime = dateTime, Callback = callback, CallbackParameter = callID });
                this.mCallTimers.Sort((x, y) => x.DateTime.CompareTo(y.DateTime));
            }
        }

        /// <summary>
        /// Removes the item timer that matches the given criteri<
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="dateTime"></param>
        public void DequeueItemTimer(int itemID, long dateTime)
        {
            lock (this.mItemTimers)
                this.mItemTimers.RemoveAll(x => x.CallbackParameter == itemID && x.DateTime == dateTime);
        }

        /// <summary>
        /// Removes the call timer that matches the given criteri<
        /// </summary>
        /// <param name="callID"></param>
        public void DequeueCallTimer(int callID)
        {
            lock (this.mCallTimers)
                this.mCallTimers.RemoveAll(x => x.CallbackParameter == callID);
        }
        
        /// <summary>
        /// Main body of the timer thread
        /// </summary>
        private void Run()
        {
            Log.Info("Timer thread started");

            try
            {
                while (true)
                {
                    // wait 1 second between ticks to keep the cpu usage low here
                    Thread.Sleep(1000);

                    long currentDateTime = DateTime.UtcNow.ToFileTimeUtc();

                    lock (this.mItemTimers)
                    {
                        foreach (Timer timer in this.mItemTimers)
                        {
                            // only iterate until we find an event that is not due to be fired
                            if (timer.DateTime > currentDateTime)
                                break;

                            Log.Debug($"Firing callback for timer on itemID {timer.CallbackParameter}");
                            
                            timer.Callback?.Invoke(timer.CallbackParameter);
                        }

                        // remove timers that have already been fired
                        this.mItemTimers.RemoveAll(x => x.DateTime <= currentDateTime);                        
                    }

                    lock (this.mCallTimers)
                    {
                        foreach (Timer timer in this.mCallTimers)
                        {
                            // only iterate until we find an event that is not due to be fired
                            if (timer.DateTime > currentDateTime)
                                break;

                            Log.Debug($"Firing callback for timer on callID {timer.CallbackParameter}");

                            timer.Callback?.Invoke(timer.CallbackParameter);
                        }
                        
                        // remove timers that have already been fired
                        this.mCallTimers.RemoveAll(x => x.DateTime <= currentDateTime);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Fatal($"Timer thread stopped: {e.Message}, timed events won't work from now on!!");
            }
        }
    }
}