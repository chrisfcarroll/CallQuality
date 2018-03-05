using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using TestBase.Shoulds;

namespace NFRInvoke.Tests
{
    [TestFixture]
    public class InvokeChainingSpecs
    {
        [Test]
        public void InvokeWrappersCanBeCombined()
        {
            var sut= new InvokeChain(
                new Cacher(TimeSpan.FromSeconds(999)), 
                new CircuitBreaker(nameof(InvokeWrappersCanBeCombined),1,TimeSpan.FromSeconds(1))
            );

            sut.Call(SucceedTwiceThenFail, 1).ShouldBe(1);
            sut.Call(SucceedTwiceThenFail, 1).ShouldBe(1);
            sut.Call(SucceedTwiceThenFail, 1).ShouldBe(1);
            sut.Call(SucceedTwiceThenFail, 1).ShouldBe(1);
            //
            SuccessfulInvocations.Count.ShouldBe(1); //Then cached
            Failures.ShouldBeEmpty();
        }

        [Test]
        public void InvokeWrappersAreCalledInLastFirstOrder()
        {
            var callTimes= new List<TimeSpan>();
            var callTimer = new CallTimer(ts=> callTimes.Add(ts));
            var cacher = new Cacher(TimeSpan.FromSeconds(999));
            var sut= new InvokeChain(callTimer, cacher             );

            sut.Call(SleepAndReturn(), 100).ShouldBe(100);
            sut.Call(SleepAndReturn(), 100).ShouldBe(100); 
            sut.Call(SleepAndReturn(), 100).ShouldBe(100);
            callTimes.Count.ShouldBe(3);
            

            var otherWayRound= new InvokeChain(cacher, callTimer);
            callTimes.Clear();
            otherWayRound.Call(SleepAndReturn(), 100).ShouldBe(100);
            otherWayRound.Call(SleepAndReturn(), 100).ShouldBe(100); 
            otherWayRound.Call(SleepAndReturn(), 100).ShouldBe(100);

            callTimes.ShouldBeEmpty();
        }

        static Func<int, int> SleepAndReturn()
        {
            return i => {Thread.Sleep(i); return i;};
        }

        [SetUp]
        public void SetUp()
        {
            SuccessfulInvocations = new List<object>();
            Failures = new List<object>();
        }

        List<object> SuccessfulInvocations = new List<object>();
        List<object> Failures = new List<object>();

        object SucceedTwiceThenFail(int parameter)
        {
            if (SuccessfulInvocations.Count >= 2) {  Failures.Add("");  throw new Exception(); }
            RecordSuccess(parameter);
            return parameter;
        }

        void RecordSuccess<T>(T param){ SuccessfulInvocations.Add(param); }
    }
}
