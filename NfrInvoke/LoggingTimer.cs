using System;

namespace NfrInvoke
{
    /// <summary>Log the time to make a call</summary>
    public class Logtimer : InvokeWrapper
    {
        protected override T Invoke<T>(Func<T> callback, Delegate wrappedFunctionCall, params object[] parameters)
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var result=callback();
            loggingCallback(ToShortString(wrappedFunctionCall, parameters), timer.Elapsed);
            return result;
        }

        readonly Action<string,TimeSpan> loggingCallback;

        public Logtimer(Action<string, TimeSpan> loggingCallback) { this.loggingCallback = loggingCallback; }

        public Logtimer(ILogger logger, string messageTemplate = null)
        {
            messageTemplate = messageTemplate ?? "Timer: {invocation} took {timeElapsed}";
            loggingCallback = (m,t)=>logger.Verbose( messageTemplate, m,t);
        }
    }
}