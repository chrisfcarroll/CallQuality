using System;
using System.Collections.Generic;
using NUnit.Framework;
using TestBase.Shoulds;

namespace NFRInvoke.Tests
{
    [TestFixture]
    public class InvokeWrapper_Specs
    {
        class InvokeTest : InvokeWrapper
        {
            protected override T Invoke<T>(Func<T> callback, Delegate wrappedFunctionCall, params object[] originalParameters)
            {
                return callback();
            }
        }

        [Test]
        public void InvokeWrapperDo__CallsTheWrappedCall()
        {
            new InvokeTest().Do(RecordInvocation, 1);
            new InvokeTest().Do(RecordInvocation, "Two");
            //
            Invocations.ShouldEqualByValue(new List<object>{1,"Two"});
        }

        void RecordInvocation<T>(T param){ Invocations.Add(param); }

        List<object> Invocations = new List<object>();
    }
}
