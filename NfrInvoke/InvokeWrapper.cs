using System;
using System.Reflection;

namespace NFRInvoke
{
    /// <summary>
    /// Wrap calls to a method so that NFRs such as caching, circuit break, logging, timing can be applied
    /// </summary>
    public abstract class InvokeWrapper
    {
        /// <summary> Override this to intercept the function call. </summary>
        /// <param name="callback">You must call this with no parameters - <paramref name="callback"/>() - to invoke the method or function being wrapped.</param>
        /// <param name="wrappedFunctionCall">Info about the method or function being wrapped.</param>
        /// <param name="originalParameters"></param>
        /// <returns></returns>
        protected abstract T Invoke<T>(Func<T> callback, Delegate wrappedFunctionCall, params object[] originalParameters);

        public TR Call<TR>(Func<TR> callback) { return Invoke(callback, callback); }

        public TR Call<T1, TR>(Func<T1, TR> callback, T1 param)
        {
            return Invoke(() => callback(param), callback, param);
        }

        public TR Call<T1, T2, TR>(Func<T1, T2, TR> callback, T1 param, T2 param2)
        {
            return Invoke(() => callback(param, param2), callback, param, param2);
        }
        public TR Call<T1, T2, T3, TR>(Func<T1, T2, T3, TR> callback, T1 param, T2 param2, T3 param3)
        {
            return Invoke(() => callback(param, param2, param3), callback, param, param2, param3);
        }
        public TR Call<T1, T2, T3, T4, TR>(Func<T1, T2, T3, T4, TR> callback, T1 param, T2 param2, T3 param3, T4 param4)
        {
            return Invoke(() => callback(param, param2, param3, param4), callback, param, param2, param3, param4);
        }
        public TR Call<T1, T2, T3, T4, T5, TR>(Func<T1, T2, T3, T4, T5, TR> callback, T1 param, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            return Invoke(() => callback(param, param2, param3, param4, param5), callback, param, param2, param3, param4, param5);
        }
        public TR Call<T1, T2, T3, T4, T5, T6, TR>(Func<T1, T2, T3, T4, T5, T6, TR> callback, T1 param, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6)
        {
            return Invoke(() => callback(param, param2, param3, param4, param5, param6), callback, param, param2, param3, param4, param5, param6);
        }
        public TR Call<T1, T2, T3, T4, T5, T6, T7, TR>(Func<T1, T2, T3, T4, T5, T6, T7, TR> callback, T1 param, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7)
        {
            return Invoke(() => callback(param, param2, param3, param4, param5, param6, param7), callback, param, param2, param3, param4, param5, param6, param7);
        }

        public void Do(Action callback)
        {
            Invoke(() => { callback(); return 0; }, callback);
        }

        public void Do<T1>(Action<T1> callback, T1 param)
        {
            Invoke(() => { callback(param); return 0;}, callback, param);
        }

        public void Do<T1, T2>(Action<T1, T2> callback, T1 param, T2 param2)
        {
            Invoke(() => { callback(param, param2); return 0; }, callback, param, param2);
        }
        public void Do<T1, T2, T3>(Action<T1, T2, T3> callback, T1 param, T2 param2, T3 param3)
        {
            Invoke(() => { callback(param, param2, param3); return 0; }, callback, param, param2, param3);
        }
        public void Do<T1, T2, T3, T4>(Action<T1, T2, T3, T4> callback, T1 param, T2 param2, T3 param3, T4 param4)
        {
            Invoke(() => { callback(param, param2, param3, param4); return 0; }, callback, param, param2, param3, param4);
        }
        public void Do<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> callback, T1 param, T2 param2, T3 param3, T4 param4, T5 param5)
        {
            Invoke(() => { callback(param, param2, param3, param4, param5); return 0; }, callback, param, param2, param3, param4, param5);
        }
        public void Do<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> callback, T1 param, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6)
        {
            Invoke(() => { callback(param, param2, param3, param4, param5, param6); return 0; }, callback, param, param2, param3, param4, param5, param6);
        }
        public void Do<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> callback, T1 param, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7)
        {
            Invoke(() => { callback(param, param2, param3, param4, param5, param6, param7); return 0; }, callback, param, param2, param3, param4, param5, param6, param7);
        }

        /// <summary>
        /// This method is intended to be used from within <see cref="Invoke{T}"/>, where it has access to the parameters needed to be useful.
        /// but can only succeed in this if the parameter Types having suitable ToString() overrides
        /// </summary>
        /// <param name="wrappedFunctionCall"></param>
        /// <param name="parameters">the parameters to be passed to the wrappedFunctionCall</param>
        /// <returns>A string representation of an invocation of <paramref name="wrappedFunctionCall"/> on <paramref name="parameters"/></returns>
        public string ToString(Delegate wrappedFunctionCall, params object[] parameters) { return ToString(wrappedFunctionCall.Method, parameters); }

        /// <summary>
        /// This method is intended to be used from within <see cref="Invoke{T}"/>, where it has access to the parameters needed to be useful.
        /// It is intended to be suitable for using as a unique key for any given function invocation but can only succeed in this 
        /// if the parameter Types having suitable ToString() overrides
        /// </summary>
        /// <param name="wrappedFunctionCallMethodInfo">Typically obtained by calling <see cref="Delegate.Method"/> on the callback function</param>
        /// <param name="parameters">the parameters to be passed to the wrappedFunctionCall</param>
        /// <returns>A string representation of an invocation of <paramref name="wrappedFunctionCallMethodInfo"/> on <paramref name="parameters"/></returns>
        public string ToString(MethodInfo wrappedFunctionCallMethodInfo, params object[] parameters)
        {
            var methodInfo = wrappedFunctionCallMethodInfo;
            var hasType = methodInfo.DeclaringType != null;
            var typeName =     hasType ? methodInfo.DeclaringType.FullName : "";
            var assemblyName = hasType ? methodInfo.DeclaringType.AssemblyQualifiedName?.Replace(methodInfo.DeclaringType?.FullName ?? "", "").TrimStart(',', ' ') : null;
            return string.Format("{0}{1}({2}):{3}",
                        typeName,
                        methodInfo.Name,
                        string.Join(", ", parameters),
                        assemblyName ?? methodInfo.GetHashCode().ToString());
        }
        /// <summary>
        /// This method is intended to be used from within <see cref="Invoke{T}"/>, where it has access to the parameters needed to be useful.
        /// but can only succeed in this if the parameter Types having suitable ToString() overrides
        /// </summary>
        /// <param name="wrappedFunctionCall"></param>
        /// <param name="parameters">the parameters to be passed to the wrappedFunctionCall</param>
        /// <returns>A string representation of an invocation of <paramref name="wrappedFunctionCall"/> on <paramref name="parameters"/></returns>
        public string ToShortString(Delegate wrappedFunctionCall, params object[] parameters) { return ToShortString(wrappedFunctionCall.Method, parameters); }

        /// <summary>
        /// This method is intended to be used from within <see cref="Invoke{T}"/>, where it has access to the parameters needed to be useful.
        /// It is intended to be suitable for using as a unique key for any given function invocation but can only succeed in this 
        /// if the parameter Types having suitable ToString() overrides
        /// </summary>
        /// <param name="wrappedFunctionCallMethodInfo">Typically obtained by calling <see cref="Delegate.Method"/> on the callback function</param>
        /// <param name="parameters">the parameters to be passed to the wrappedFunctionCall</param>
        /// <returns>A string representation of an invocation of <paramref name="wrappedFunctionCallMethodInfo"/> on <paramref name="parameters"/></returns>
        public string ToShortString(MethodInfo wrappedFunctionCallMethodInfo, params object[] parameters)
        {
            var hasType = wrappedFunctionCallMethodInfo.DeclaringType != null;
            var typeName = hasType ? wrappedFunctionCallMethodInfo.DeclaringType.Name : "";

            return string.Format("{0}{1}({2})",
                        typeName,
                        wrappedFunctionCallMethodInfo.Name,
                        string.Join(", ", parameters));
        }
    }
}