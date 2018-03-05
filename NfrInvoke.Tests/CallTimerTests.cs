using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using TestBase.Shoulds;

namespace NFRInvoke.Tests
{
    [TestFixture]
    public class CallTimerTests
    {

        [Test]
        public void CallTimer_TimesCalls()
        {
            var callTimes= new List<TimeSpan>();
            var callTimer = new CallTimer(ts=> callTimes.Add(ts));

            callTimer.Call(SleepAndReturn(), 200).ShouldBe(200);
            callTimer.Call(SleepAndReturn(), 200).ShouldBe(200); 
            callTimer.Call(SleepAndReturn(), 200).ShouldBe(200);

            callTimes.ForEach(t=>Console.WriteLine(t));

            callTimes.Count.ShouldBe(3);
            callTimes.ShouldAll(
                ts => ts.ShouldBeBetween(TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(1000))
                );
            callTimer.LastCallTime.ShouldBe(callTimes.Last());
        }

        static Func<int, int> SleepAndReturn()
        {
            return i => {Thread.Sleep(i); return i;};
        }
    }
}
