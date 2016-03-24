using System;
using System.Collections.Generic;
using NUnit.Framework;
using TestBase.Shoulds;

namespace NfrInvoke.Tests
{
    [TestFixture]
    public class InvokeWrapperAssertions
    {
        class InvokeTest : InvokeWrapper
        {
            protected override T Invoke<T>(Func<T> callback, Delegate wrappedFunctionCall, params object[] parameters)
            {
                Invocations.Add(ToString(wrappedFunctionCall, parameters));
                return callback();
            }

            public List<string> Invocations = new List<string>();
        }


        [Test]
        public void InvokeWrapperDo__CallsTheWrappedCall()
        {
            new InvokeTest().Do(RecordInvocation, 1);
            //
            Invocations.ShouldEqualByValue(new []{1});
        }


        void RecordInvocation<T>(T param)
        {
            Invocations.Add(param);    
        }

        public List<object> Invocations = new List<object>();
    }
}
