using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NfrInvoke
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
        public static readonly int DefaultBreakForSeconds = Properties.Settings.Default.CMSDatabaseCircuitBreakBreaksForSeconds;
        public static readonly int DefaultErrorsBeforeBreaking = Properties.Settings.Default.CMSDatabaseCircuitBreaksAfterNErrors;

        /// <summary>If the breaker trips, how long to wait before retrying</summary>
        public int BreakForSeconds { get; set; }
        /// <summary>How many errors to tolerate before breaking</summary>
        public int ErrorsBeforeBreaking { get; private set; }

        public CircuitBreaker(string circuitName, Type[] exceptionsToThrowNotBreak = null, Type[] exceptionsToBreak = null, int? errorsBeforeBreaking = null, int? breakForSeconds = null, ILogger seriLogger = null)
        {
            logger = seriLogger??Log.Logger;
            this.exceptionsToBreak = exceptionsToBreak??new[] {typeof (Exception)};
            exceptionTypesToThrowNotBreak = exceptionsToThrowNotBreak ?? new Type[0];
            this.circuitName = circuitName;
            BreakForSeconds = breakForSeconds??DefaultBreakForSeconds;
            ErrorsBeforeBreaking = errorsBeforeBreaking ?? DefaultErrorsBeforeBreaking;
            if (!lastErrors.ContainsKey(circuitName)) { lastErrors[circuitName] = new CircularQueue<DateTime>(ErrorsBeforeBreaking); }
        }

        protected override T Invoke<T>(Func<T> callback, Delegate wrappedFunctionCall, params object[] parameters)
        {
            if ((DateTime.Now - lastErrors[circuitName].Peek()).TotalSeconds > BreakForSeconds)
            {
                try
                {
                    logger.Verbose("CircuitBreaker {circuitKeyName} trying {functionCall} at " + DateTime.Now, circuitName, ToShortString(wrappedFunctionCall,parameters));
                    //
                    var result = callback();
                    lastErrors[circuitName].Empty();
                    return result;
                }
                catch (Exception e)
                {
                    if (exceptionsToBreak.Any(et=>et.IsInstanceOfType(e)  && !exceptionTypesToThrowNotBreak.Contains(e.GetType())))
                    {
                        lastErrors[circuitName].Push(DateTime.Now);
                        logger.Warning(e, "CircuitBreaker {circuitKeyName} caught exception." + LoggingConfig.Template, circuitName);
                    }
                    else
                    {
                        logger.Verbose(e, "CircuitBreaker {circuitKeyName} is rethrowing because an exception in its exceptionTypesToThrowNotBreak list." + LoggingConfig.Template, circuitName);
                        throw;
                    }
                }
            }
            else
            {
                Log.Warning("CircuitBreaker {circuitKeyName} broke call to {method}. {BreakForSeconds}, {ErrorsBeforeBreaking}" + LoggingConfig.Template,
                            circuitName, MethodName(callback.Method), BreakForSeconds, ErrorsBeforeBreaking);
            }

            return EmptyCollectionOrDefault<T>();
        }

        T EmptyCollectionOrDefault<T>()
        {
            try
            {
                var t = typeof(T);
                if (t.IsArray)
                {
                    return (T)(object)Array.CreateInstance(t.GetElementType(), 0);
                }
                else if (typeof (IEnumerable).IsAssignableFrom(t))
                {
                    if (t.IsInterface)
                    {
                        //this only copes with the case where t is IEnumerable<TItem>. Otherwise throw
                        var listof = typeof (List<>);
                        var typetomake = listof.MakeGenericType(t.GetGenericArguments());
                        return (T)Activator.CreateInstance(typetomake);
                    }
                    return Activator.CreateInstance<T>();
                }
                else 
                { 
                    return default(T);
                }
            }
            catch (Exception e)
            {
                var ae= new ApplicationException("Failed to make an instance of " + typeof(T), e);
                logger.Information(ae, LoggingConfig.Template);
                return default(T);
            }
        }

        readonly ILogger logger;
        readonly string circuitName;
        readonly Type[] exceptionsToBreak;
        readonly Type[] exceptionTypesToThrowNotBreak;
        static readonly Dictionary<string, CircularQueue<DateTime>> lastErrors = new Dictionary<string, CircularQueue<DateTime>>();

        string MethodName(MethodInfo methodInfo)
        {
            var hasType = methodInfo.DeclaringType != null;
            var typeName = hasType ? methodInfo.DeclaringType.FullName : methodInfo.GetHashCode().ToString();
            return string.Format("{0}.{1}", typeName, methodInfo.Name);
        }

        public static void Forget(string circuitName) { if(lastErrors.ContainsKey(circuitName)){lastErrors.Remove(circuitName);}}
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