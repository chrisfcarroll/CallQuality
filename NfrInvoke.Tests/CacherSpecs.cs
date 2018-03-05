using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using TestBase.Shoulds;

namespace NFRInvoke.Tests
{
    [TestFixture]
    public class CacherSpecs
    {
        [Test]
        public void Cacher__CallsTheWrappedCall()
        {
            new Cacher(TimeSpan.FromSeconds(99)).Call(SucceedNTimesThenFail, 999).ShouldBe(999);
        }

        [Test]
        public void Cacher__Caches()
        {
            var cacher = new Cacher(TimeSpan.FromSeconds(99));

            var r1=cacher.Call(SucceedNTimesThenFail, 2);
            var r2=cacher.Call(SucceedNTimesThenFail, 2);
            var r3=cacher.Call(SucceedNTimesThenFail, 2);
            var r4=cacher.Call(SucceedNTimesThenFail, 2);

            SuccessfulInvocations.Count.ShouldBe(1);
            r1.ShouldEqual(r2).ShouldEqual(r3).ShouldEqual(r4).ShouldBe(2);
        }

        [Test]
        public void Cacher__Forgets()
        {
            var cacher = new Cacher(TimeSpan.FromMilliseconds(1), nameof(Cacher__Forgets));

            cacher.Call(SucceedNTimesThenFail, 2);
            Thread.Sleep(100);
            cacher.Call(SucceedNTimesThenFail, 2);

            SuccessfulInvocations.Count.ShouldBe(2);
        }

        [Test]
        public void Cacher__WiththeSameNameSharesCachedEntries()
        {
            var cacher1 = new Cacher(TimeSpan.FromMinutes(1), nameof(Cacher__WiththeSameNameSharesCachedEntries));
            var cacher2 = new Cacher(TimeSpan.FromMinutes(1), nameof(Cacher__WiththeSameNameSharesCachedEntries));

            cacher1.Call(SucceedNTimesThenFail, 2);
            Thread.Sleep(100);
            cacher2.Call(SucceedNTimesThenFail, 2);

            SuccessfulInvocations.Count.ShouldBe(1);
        }

        [Test]
        public void Cacher__WithDifferentNameDoesntShareCachedEntries()
        {
            var cacher1 = new Cacher(TimeSpan.FromMinutes(1), new Random(1).Next().ToString());
            var cacher2 = new Cacher(TimeSpan.FromMinutes(1), new Random(2).Next().ToString());

            cacher1.Call(SucceedNTimesThenFail, 2);
            cacher2.Call(SucceedNTimesThenFail, 2);

            SuccessfulInvocations.Count.ShouldBe(2);
        }

        
        [SetUp]
        public void SetUp()
        {
            SuccessfulInvocations = new List<object>();
            Failures = new List<object>();
        }

        List<object> SuccessfulInvocations = new List<object>();
        List<object> Failures = new List<object>();

        int SucceedNTimesThenFail(int times)
        {
            if (SuccessfulInvocations.Count >= times){ Failures.Add(""); throw new Exception(); }
            RecordSuccess(times);
            return times;
        }

        void RecordSuccess<T>(T param){ SuccessfulInvocations.Add(param); }

    }
}
