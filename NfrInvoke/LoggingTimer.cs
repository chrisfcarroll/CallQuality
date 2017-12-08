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
            loggingCallback(timer.Elapsed);
            return result;
        }

        readonly Action<TimeSpan> loggingCallback;

        public CallTimer(Action<TimeSpan> loggingCallback){this.loggingCallback = loggingCallback;}
    }
}