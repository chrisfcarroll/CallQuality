using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NFRInvoke
{
    /// <summary>Wrap calls in a try catch. On failure, log the exception and swallow it, returning:
    /// <list type="bullet">
    /// <item>an empty collection, if the return type is a Collection type</item>
    /// <item>a default object otherwise</item>
    /// </list>
    /// On repeated failure, "break the circuit" : stop calling the function for a timeout period before trying again
    /// </summary>
    public class CircuitBreaker : InvokeWrapper
    {
        public static readonly int DefaultBreakForSeconds = 15;
        public static readonly int DefaultErrorsBeforeBreaking = 2;

        /// <summary>A name for the circuit. Multiple breakers for the same CircuitName will share breakage history</summary>
        public string CircuitName { get; }
        /// <summary>How many errors does this circuit tolerate before breaking. This value cannot be reset after the first breaker for a <see cref="CircuitName"/> has set it.</summary>
        public int ErrorsBeforeBreaking { get; }
        /// <summary>If the circuit <see cref="CircuitName"/> errors, how long will this breaker be tripped before retrying</summary>
        public int BreakForSeconds { get; set; }

        /// <summary>Forget all state – errors and timings – being remembered for the circuit <paramref name="circuitName"/> and reset the <see cref="ErrorsBeforeBreaking"/> size.</summary>
        public static void Reset(string circuitName, int? errorsBeforeBreaking=null) { lastErrors[circuitName] = new CircularQueue<DateTime>(errorsBeforeBreaking??DefaultErrorsBeforeBreaking); }

        /// <summary>
        /// Creates a breaker for the circuit <paramref name="circuitName"/>
        /// </summary>
        /// <param name="circuitName">Identifies the circuit. Multiple instances of <see cref="CircuitBreaker"/> for the same <paramref name="circuitName"/> 
        /// will break the same circuit.</param>
        /// <param name="errorsBeforeBreaking">This value is shared between all breakers for the same circuit. Only the first breaker created for a circuit can set this value. Subsequent values will be ignored.</param>
        /// <param name="breakForSeconds">This value is specific to this breaker.</param>
        /// <param name="exceptionsToThrowNotBreak"></param>
        /// <param name="exceptionsToBreak"></param>
        /// <param name="logCallAndParametersBeforeCall"></param>
        /// <param name="logExceptionCaught"></param>
        /// <param name="logExceptionWillBeThrown"></param>
        /// <param name="logDroppedCallWhileCircuitBroken"></param>
        public CircuitBreaker(string circuitName, int? errorsBeforeBreaking = null, int? breakForSeconds = null, Type[] exceptionsToThrowNotBreak = null, Type[] exceptionsToBreak = null, 
                                Action<string> logCallAndParametersBeforeCall=null, 
                                Action<Exception> logExceptionCaught = null, 
                                Action<Exception> logExceptionWillBeThrown = null,
                                Action logDroppedCallWhileCircuitBroken = null)
        {
            this.CircuitName = circuitName;
            ErrorsBeforeBreaking = errorsBeforeBreaking ?? DefaultErrorsBeforeBreaking;
            BreakForSeconds = breakForSeconds ?? DefaultBreakForSeconds;
            exceptionTypesToThrowNotBreak = exceptionsToThrowNotBreak ?? new Type[0];
            this.logCallAndParametersBeforeCall = logCallAndParametersBeforeCall ?? (s => { });
            this.logExceptionWillBeThrown = logExceptionCaught ?? (e => { });
            this.exceptionsToBreak = exceptionsToBreak??new[] {typeof (Exception)};
            this.logDroppedCallWhileCircuitBroken = logDroppedCallWhileCircuitBroken;
            this.logExceptionWillBeThrown = logExceptionWillBeThrown;
            if (!lastErrors.ContainsKey(circuitName)) { lastErrors[circuitName] = new CircularQueue<DateTime>(ErrorsBeforeBreaking); }
        }

        protected override T Invoke<T>(Func<T> callback, Delegate wrappedFunctionCall, params object[] parameters)
        {
            if ((DateTime.Now - lastErrors[CircuitName].Peek()).TotalSeconds > BreakForSeconds)
            {
                try
                {
                    logCallAndParametersBeforeCall(ToShortString(wrappedFunctionCall,parameters));
                    //
                    var result = callback();
                    lastErrors[CircuitName].Empty();
                    return result;
                }
                catch (Exception e)
                {
                    if (exceptionsToBreak.Any(et=>et.IsInstanceOfType(e)  && !exceptionTypesToThrowNotBreak.Contains(e.GetType())))
                    {
                        lastErrors[CircuitName].Push(DateTime.Now);
                        logExceptionWillBeThrown(e);
                    }
                    else { logExceptionWillBeThrown(e); throw; }
                }
            }
            else { logDroppedCallWhileCircuitBroken(); }

            return EmptyCollectionOrDefault<T>();
        }

        T EmptyCollectionOrDefault<T>()
        {
            try
            {
                var t = typeof(T);
                if (t.IsArray)
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    return (T)(object)Array.CreateInstance(t.GetElementType(), 0);
                }
                if (typeof (IEnumerable).IsAssignableFrom(t))
                {
                    if (t.IsInterface)
                    {
                        //this only copes with the case where t is IEnumerable<TItem>
                        var listof = typeof (List<>);
                        var typetomake = listof.MakeGenericType(t.GetGenericArguments());
                        return (T)Activator.CreateInstance(typetomake);
                    }
                    return Activator.CreateInstance<T>();
                }
            }
            catch{}
            return default(T);
        }

        readonly Action logDroppedCallWhileCircuitBroken;
        readonly Type[] exceptionsToBreak;
        readonly Type[] exceptionTypesToThrowNotBreak;
        static readonly Dictionary<string, CircularQueue<DateTime>> lastErrors = new Dictionary<string, CircularQueue<DateTime>>();
        Action<string> logCallAndParametersBeforeCall;
        Action<Exception> logExceptionWillBeThrown;
    }

    class CircularQueue<T>
    {
        readonly int circleSize;
        readonly Queue<T> queue;
        readonly object locker= new object();

        public T Push(T item)
        {
            lock (locker)
            {
                queue.Enqueue(item);
                return queue.Dequeue();
            }
        }
        // ReSharper disable once InconsistentlySynchronizedField
        public T Peek() { return queue.Peek(); }

        public CircularQueue(int circleSize, T initialFill= default(T))
        {
            this.circleSize = circleSize;
            queue=new Queue<T>(circleSize);
            Fill(initialFill); 
        }
        public void Fill(T value) { lock(locker){queue.Clear(); for(int i=0; i<circleSize; i++){queue.Enqueue(value);}} }
        public void Empty() { Fill(default(T)); }
    }
}