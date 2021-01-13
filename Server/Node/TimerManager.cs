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
        public int ItemID;
    };
    
    public class TimerManager
    {
        private Thread mThread = null;
        private Channel Log = null;
        private List<Timer> mTimers = new List<Timer>();
        
        public TimerManager(Logger logger)
        {
            this.Log = logger.CreateLogChannel("TimerManager");
            this.mThread = new Thread(Run);
        }

        public void Start()
        {
            this.mThread.Start();
        }
        
        public void EnqueueTimer(long dateTime, Action<int> callback, int itemID)
        {
            lock (this.mTimers)
            {
                this.mTimers.Add(new Timer () { DateTime = dateTime, Callback = callback, ItemID = itemID });
                // reorder the list
                this.mTimers.Sort((x, y) => x.DateTime.CompareTo(y.DateTime));    
            }
        }

        public void DequeueTimer(int itemID, long dateTime)
        {
            lock (this.mTimers)
            {
                this.mTimers.RemoveAll(x => x.ItemID == itemID && x.DateTime == dateTime);
            }
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

                    lock (this.mTimers)
                    {
                        foreach (Timer timer in this.mTimers)
                        {
                            // only iterate until we find an event that is not due to be fired
                            if (timer.DateTime > currentDateTime)
                                break;

                            Log.Debug($"Firing callback for timer on itemID {timer.ItemID}");
                            
                            timer.Callback.Invoke(timer.ItemID);
                        }

                        // remove timers that have already been fired
                        this.mTimers.RemoveAll(x => x.DateTime <= currentDateTime);                        
                    }
                }
            }
            catch (Exception e)
            {
                Log.Fatal($"Timer thread stopped: {e.Message}, timed events won't work");
            }
        }
    }
}