using System;

namespace NFRInvoke
{
    /// <summary>Return the time to make a call</summary>
    public class CallTimer : InvokeWrapper
    {
        protected override T Invoke<T>(Func<T> callback, Delegate wrappedFunctionCall, params object[] parameters)
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var result=callback();
            onCallEndedAndTimed(LastCallTime=timer.Elapsed);
            return result;
        }

        /// <summary>The elapsed time of the most recent successful call.</summary>
        public TimeSpan LastCallTime { get; private set; }

        readonly Action<TimeSpan> onCallEndedAndTimed;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onCallEndedAndTimed">This callback will receive the time elapsed.</param>
        public CallTimer(Action<TimeSpan> onCallEndedAndTimed){this.onCallEndedAndTimed = onCallEndedAndTimed;}
    }
}