using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Common.Logging;

namespace Node
{
    class Timer
    {
        public long DateTime;
        public Action<int> Callback;
        public int CallbackParameter;
    };
    
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

        public void Start()
        {
            this.mThread.Start();
        }
        
        public void EnqueueItemTimer(long dateTime, Action<int> callback, int itemID)
        {
            lock (this.mItemTimers)
            {
                this.mItemTimers.Add(new Timer () { DateTime = dateTime, Callback = callback, CallbackParameter = itemID });
                // reorder the list
                this.mItemTimers.Sort((x, y) => x.DateTime.CompareTo(y.DateTime));
            }
        }

        public void EnqueueCallTimer(long dateTime, Action<int> callback, int callID)
        {
            lock (this.mCallTimers)
            {
                this.mCallTimers.Add(new Timer () { DateTime = dateTime, Callback = callback, CallbackParameter = callID });
                this.mCallTimers.Sort((x, y) => x.DateTime.CompareTo(y.DateTime));
            }
        }

        public void DequeueItemTimer(int itemID, long dateTime)
        {
            lock (this.mItemTimers)
                this.mItemTimers.RemoveAll(x => x.CallbackParameter == itemID && x.DateTime == dateTime);
        }

        public void DequeueCallTimer(int callID)
        {
            lock (this.mCallTimers)
                this.mCallTimers.RemoveAll(x => x.CallbackParameter == callID);
        }
        
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