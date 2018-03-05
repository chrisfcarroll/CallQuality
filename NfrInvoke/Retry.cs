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
                    onExceptionDuringRetry(e);
                    Thread.Sleep(waitRetryAlgorithm());
                } 
            }
            return callback();
        }

        readonly TimeSpan timeout;
        readonly Func<TimeSpan> waitRetryAlgorithm;
        readonly int maxRetries;
        readonly Action<Exception> onExceptionDuringRetry;

        /// <param name="timeout">The time to keep retrying. Once this time is expired,a final attempt will be made to make the call. Any exceptions raised by this final try will be thrown.</param>
        /// <param name="waitRetryAlgorithm">This function will be called once for each failure, and indicates how long to wait until the next retry.</param>
        /// <param name="maxRetries">Note that maximum zero <strong>retries</strong> still means 1 try</param>
        /// <param name="onExceptionDuringRetry">This action is called when an exception is thrown by an attempted call, but we haven't finished retrying yet.</param>
        /// <remarks>Retry will catch exceptions while it is retrying. Exception can be seen by using the <paramref name="onExceptionDuringRetry"/> callback. </remarks>
        public Retry(TimeSpan timeout, Func<TimeSpan> waitRetryAlgorithm, int maxRetries = int.MaxValue, Action<Exception> onExceptionDuringRetry = null)
        {
            this.timeout = timeout > TimeSpan.Zero ? timeout : TimeSpan.MaxValue;
            this.waitRetryAlgorithm = waitRetryAlgorithm;
            this.maxRetries = maxRetries;
            this.onExceptionDuringRetry = onExceptionDuringRetry ?? (e => { });
        }
        public Retry(int timeoutMillis, Func<TimeSpan> waitRetryAlgorithm, int maxRetries = int.MaxValue, Action<Exception> onExceptionDuringRetry = null) : this(TimeSpan.FromMilliseconds(timeoutMillis), waitRetryAlgorithm, maxRetries, onExceptionDuringRetry) { }
        public Retry(TimeSpan timeout, TimeSpan waitRetryInterval, int maxRetries = int.MaxValue, Action<Exception> onExceptionDuringRetry=null) : this(timeout, () => waitRetryInterval, maxRetries, onExceptionDuringRetry) { }
        public Retry(int timeoutMillis, int waitRetryIntervalMillis, int maxRetries = int.MaxValue, Action<Exception> onExceptionDuringRetry = null)
                    : this(new TimeSpan(0, 0, 0, 0, timeoutMillis), new TimeSpan(0, 0, 0, 0, waitRetryIntervalMillis), maxRetries,onExceptionDuringRetry) { }

        /// <summary>Returns a retry function which doubles the wait interval each time</summary>
        public static Func<TimeSpan> ExponentialBackOff(TimeSpan firstWaitInterval)
        {
            var seed = firstWaitInterval;
            return () => seed += seed;
        }
    }
}