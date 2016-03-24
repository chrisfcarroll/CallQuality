using System;
using System.Threading;

namespace NfrInvoke
{
    class Retry : InvokeWrapper
    {
        protected override T Invoke<T>(Func<T> callback, Delegate wrappedFunctionCall, params object[] parameters)
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            while (timer.Elapsed < giveUpTimeout)
            {
                try
                {
                    return callback();
                }
                catch(Exception)
                {
                    Thread.Sleep(waitRetryInterval);
                } 
            }
            return callback();
        }

        readonly TimeSpan giveUpTimeout;
        readonly TimeSpan waitRetryInterval;

        public Retry(TimeSpan giveUpTimeout, TimeSpan waitRetryInterval)
        {
            if (waitRetryInterval.TotalMilliseconds < 1) { throw new ArgumentOutOfRangeException("waitRetryInterval","waitRetryInterval must be at least 1 millisecond");}
            this.giveUpTimeout = giveUpTimeout;
            this.waitRetryInterval = waitRetryInterval;
        }
        public Retry(int giveUpTimeoutMillis, int waitRetryIntervalMillis)
                    : this(new TimeSpan(0, 0, 0, 0, giveUpTimeoutMillis), new TimeSpan(0, 0, 0, 0, waitRetryIntervalMillis)) { }
    }
}