using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using TestBase.Shoulds;

namespace NFRInvoke.Tests
{
    [TestFixture]
    public class Retry_Specs
    {
        [Test]
        public void RetryDo__CallsTheWrappedCall()
        {
            new Retry(1,1).Do(RecordInvocation, 99);
            //
            SuccessfulInvocations.Count.ShouldBe(1);
            SuccessfulInvocations[0].ShouldBe(99);
        }

        [TestCase(1000)]
        public void RetryDo__AttemptsToCallAndCatchesExceptions(int timeout)
        {
            var sut = new Retry(timeout, 1, logExceptionCaught: e=>Failures.Add(e));
            var stopwatch = Stopwatch.StartNew();
            sut.Do( p=> Throw3TimesIfThereIsTimeLeftThenRecordSuccess(p,stopwatch,timeout), 1);
            //
            Failures.Count.ShouldBe(3);
            SuccessfulInvocations.Count.ShouldEqual(1);
        }

        [TestCase(1000,99)][TestCase(1000, 1)][TestCase(1000, 0)]
        public void RetryDo__ObeysMaxRetries(int timeout, int maxRetries)
        {
            var sut = new Retry(timeout, 1, maxRetries, e => Failures.Add(e));
            var stopwatch = Stopwatch.StartNew();
            sut.Do(p => ThrowNTimesIfThereIsTimeLeftThenRecordSuccess(p, stopwatch, timeout, maxRetries), 1);
            //
            Failures.Count.ShouldBe(maxRetries);
            SuccessfulInvocations.Count.ShouldEqual(1);
        }

        [TestCase(1000)]
        public void RetryDo__ObeysTheRetryWaitAlgorithm(int timeout)
        {
            var attempts = 0;
            var sut = new Retry(timeout, Retry.ExponentialBackOff(TimeSpan.FromMilliseconds(10)), logExceptionCaught: e => Failures.Add(e));
            //
            //Trial with Backoff
            var stopwatchBackoff = Stopwatch.StartNew();
            sut.Do(p => Throw3TimesIfThereIsTimeLeftThenRecordSuccess(p, stopwatchBackoff, timeout), 1);
            var timeFor3ExponentialBackOffs = stopwatchBackoff.ElapsedMilliseconds;

            //Trial with 1 tick wait
            var stopwatch1Tick = Stopwatch.StartNew();
            new Retry(timeout, () => TimeSpan.FromTicks(1)).Do(x => {if(attempts++<2){throw new Exception();} },1);
            var timeFor1TickWait = stopwatch1Tick.ElapsedMilliseconds;
            //
            timeFor1TickWait.ShouldBeLessThan(10);
            timeFor3ExponentialBackOffs.ShouldBeGreaterThan(10 + 20 + 40);
        }

        [SetUp]
        public void SetUp()
        {
            SuccessfulInvocations = new List<object>();
            Failures = new List<Exception>();
        }

        List<object> SuccessfulInvocations = new List<object>();
        List<Exception> Failures = new List<Exception>();

        void Throw3TimesIfThereIsTimeLeftThenRecordSuccess(int p, Stopwatch stopwatch, int timeoutMillis)
        {
            ThrowNTimesIfThereIsTimeLeftThenRecordSuccess(p, stopwatch, timeoutMillis, 3);
        }

        void ThrowNTimesIfThereIsTimeLeftThenRecordSuccess(int p, Stopwatch stopwatch, int timeoutMillis, int nTimes)
        {
            var thereIsStillTimeToSucceed = stopwatch.ElapsedMilliseconds < 1 + timeoutMillis;
            if (Failures.Count < nTimes && thereIsStillTimeToSucceed) { throw new Exception(); }
            RecordInvocation(p);
        }

        void RecordInvocation<T>(T param){ SuccessfulInvocations.Add(param); }

    }
}
