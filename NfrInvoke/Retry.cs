using System;
using System.Threading;

namespace NFRInvoke
{
    /// <summary>
    /// Calls a function and retries repeatedly if the call throws an Exception.
    /// Retries are limited both by a timeout and a maximumRetries value.
    /// We wait between retries, either for fixed time interval or according to a given wait algorithm.
    /// </summary>
    public class Retry : InvokeWrapper
    {
        protected override T Invoke<T>(Func<T> callback, Delegate wrappedFunctionCall, params object[] parameters)
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var tries=0;
            while (timer.Elapsed < timeout & tries++ <maxRetries)
            {
                try{ return callback(); }
                catch(Exception e)
                {
                    logExceptionCaught(e);
                    Thread.Sleep(waitRetryAlgorithm());
                } 
            }
            return callback();
        }

        readonly TimeSpan timeout;
        readonly Func<TimeSpan> waitRetryAlgorithm;
        readonly int maxRetries;
        readonly Action<Exception> logExceptionCaught;

        /// <param name="timeout"></param>
        /// <param name="waitRetryAlgorithm">This function will be called once for each failure, and indicates how long to wait until the next retry.</param>
        /// <param name="maxRetries">Note that maximum zero <strong>retries</strong> still means 1 try</param>
        /// <param name="logExceptionCaught"></param>
        public Retry(TimeSpan timeout, Func<TimeSpan> waitRetryAlgorithm, int maxRetries = int.MaxValue, Action<Exception> logExceptionCaught = null)
        {
            this.timeout = timeout > TimeSpan.Zero ? timeout : TimeSpan.MaxValue;
            this.waitRetryAlgorithm = waitRetryAlgorithm;
            this.maxRetries = maxRetries;
            this.logExceptionCaught = logExceptionCaught ?? (e => { });
        }
        public Retry(int timeoutMillis, Func<TimeSpan> waitRetryAlgorithm, int maxRetries = int.MaxValue, Action<Exception> logExceptionCaught = null) : this(TimeSpan.FromMilliseconds(timeoutMillis), waitRetryAlgorithm, maxRetries, logExceptionCaught) { }
        public Retry(TimeSpan timeout, TimeSpan waitRetryInterval, int maxRetries = int.MaxValue, Action<Exception> logExceptionCaught=null) : this(timeout, () => waitRetryInterval, maxRetries, logExceptionCaught) { }
        public Retry(int timeoutMillis, int waitRetryIntervalMillis, int maxRetries = int.MaxValue, Action<Exception> logExceptionCaught = null)
                    : this(new TimeSpan(0, 0, 0, 0, timeoutMillis), new TimeSpan(0, 0, 0, 0, waitRetryIntervalMillis), maxRetries,logExceptionCaught) { }

        /// <summary>Returns a retry function which doubles the wait interval each time</summary>
        public static Func<TimeSpan> ExponentialBackOff(TimeSpan firstWaitInterval)
        {
            var seed = firstWaitInterval;
            return () => seed += seed;
        }
    }
}