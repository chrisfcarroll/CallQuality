using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using TestBase.Shoulds;

namespace NFRInvoke.Tests
{
    [TestFixture]
    public class CircuitBreakerSpecs
    {
        [Test]
        public void CircuitBreakerDo__CallsTheWrappedCall()
        {
            new CircuitBreaker("Test1",1,TimeSpan.FromSeconds(1)).Do(RecordSuccess, 99);
            //
            SuccessfulInvocations.Count.ShouldBe(1);
            SuccessfulInvocations[0].ShouldBe(99);
        }

        [Test]
        public void CircuitBreaker__ResumesCallsAfterTimeout()
        {
            var sut = new CircuitBreaker(nameof(CircuitBreaker__ResumesCallsAfterTimeout), 2, TimeSpan.FromSeconds(1), onDroppedCallWhileCircuitBroken: ()=>droppedCalls++);

            sut.Do(()=>FailNTimesThenSucceed(99, "F")); //Fail
            sut.Do(()=>FailNTimesThenSucceed(99, "F")); //Fail 2nd time
            sut.Do(()=>FailNTimesThenSucceed(99, "D")); //Drop
            Thread.Sleep(1001);
            sut.Do(()=>FailNTimesThenSucceed(99, "F")); //Fail again
            sut.Do(()=>FailNTimesThenSucceed(99, "F")); //Fail 2nd time again
            sut.Do(()=>FailNTimesThenSucceed(99, "D")); //Drop
            sut.Do(()=>FailNTimesThenSucceed(99, "D")); //Drop

            Failures.Count.ShouldBe(4);
            droppedCalls.ShouldBe(3);
            SuccessfulInvocations.Count.ShouldEqual(0);
        }

        [TestCase(1),TestCase(2),TestCase(8)]
        public void CircuitBreaker__DropsCallsWhenBroken(int errorsBeforeBreaking)
        {
            var sut = new CircuitBreaker(
                nameof(CircuitBreaker__DropsCallsWhenBroken) + errorsBeforeBreaking, 
                errorsBeforeBreaking, 
                TimeSpan.MaxValue, 
                onDroppedCallWhileCircuitBroken: ()=>droppedCalls++);

            for (int i = 0; i < 10; i++)
            {
                sut.Do(SucceedTwiceThenFail);
            }
            
            SuccessfulInvocations.Count.ShouldEqual(2);
            Failures.Count.ShouldBe(errorsBeforeBreaking);
            droppedCalls.ShouldBe(10 -2 -errorsBeforeBreaking);
        }

        [Test]
        public void CircuitBreaker__DropsCallsWhenBroken__GivenImmediateFailure()
        {
            var sut = new CircuitBreaker("Test2", 1, TimeSpan.MaxValue, onDroppedCallWhileCircuitBroken: ()=>droppedCalls++);
            for (int i = 0; i < 10; i++)
            {
                sut.Do(p => Fail3TimesThenSucceed(p), 1);
            }
            
            Failures.Count.ShouldBe(1);
            SuccessfulInvocations.Count.ShouldEqual(0);
            droppedCalls.ShouldBe(9);
        }

        [Test]
        public void CircuitBreaker__DropsCallsBasedOnCircuitName()
        {
            for (int i = 0; i < 10; i++)
            {
                new CircuitBreaker("Test3", 1, TimeSpan.MaxValue, onDroppedCallWhileCircuitBroken: () => droppedCalls++).Do(p => Fail3TimesThenSucceed(p),1);
            }

            Failures.Count.ShouldBe(1);
            SuccessfulInvocations.Count.ShouldEqual(0);
            droppedCalls.ShouldBe(9);
        }
        
        [SetUp]
        public void SetUp()
        {
            SuccessfulInvocations = new List<object>();
            Failures = new List<object>();
            droppedCalls = 0;
        }

        List<object> SuccessfulInvocations = new List<object>();
        List<object> Failures = new List<object>();
        int droppedCalls;

        void SucceedTwiceThenFail()
        {
            if (SuccessfulInvocations.Count >= 2) {  Failures.Add("");  throw new Exception(); }
            RecordSuccess("");
        }

        void Fail3TimesThenSucceed(object parameter)
        {
            FailNTimesThenSucceed(3, parameter);
        }

        void FailNTimesThenSucceed(int nTimes, object p)
        {
            if (Failures.Count < nTimes ) {  Failures.Add(p);  throw new Exception(); }
            RecordSuccess(p);
        }

        void RecordSuccess<T>(T param){ SuccessfulInvocations.Add(param); }

    }
}
