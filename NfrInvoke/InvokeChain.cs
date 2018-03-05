using System;
using System.Linq;

namespace NFRInvoke
{
    /// <summary>Chain--that is to say, apply in order-- multiple <see cref="InvokeWrapper"/>s .</summary>
    /// <remarks>The order is as normal function invocation; the leftmost is the outermost function and hence the last to be applied.
    /// i.e. `InvokeChain(a,b,c).Call(Method)` is functionally equivalent to `a( b( c(Method())))`
    /// </remarks>
    public class InvokeChain : InvokeWrapper
    {
        readonly InvokeWrapper[] invokeWrappers;

        protected override T Invoke<T>(Func<T> callback, Delegate wrappedFunctionCall, params object[] parameters)
        {
            Func<T> wrappedcallback = callback;
            foreach (var wrapper in invokeWrappers.Reverse())
            {
                Func<T> wrappedcallbackN = wrappedcallback;
                wrappedcallback = ()=>wrapper.ChainableInvoke(wrappedcallbackN, wrappedFunctionCall, parameters);
            }
            return wrappedcallback();
        }

        public InvokeChain(params InvokeWrapper[] invokeWrappers){ this.invokeWrappers = invokeWrappers; }
    }
}