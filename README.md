# NFRInvoke : a simple pattern for function-call NFRs : retry, circuit-breaker, caching, timing, logging

NFRInvoke provides a simple pattern for addressing software quality requirements, or NFRS, or cross-cutting concerns, around distributed calls. Use it in where you can't or won't use MSIL rewrite techniques.

Currently includes these InvokeWrappers:

* CircuitBreaker
* CallTimer
* Cacher
* Retry

Examples
```
var cacher = new Cacher(TimeSpan.FromSeconds(99));
var r1=cacher.Call(ExpensiveMethod);
var r2=cacher.Call(ExpensiveMethod);

var breaker= new CircuitBreaker(""Test1"",1,TimeSpan.FromSeconds(1));
var r3=breaker.Call(Method1, p1,p2); 
var r4=breaker.Call(Method2, p1,p2,p3);

var r5= new Retry(timeout, maxRetries: 3).Do(Method3, 1)
var backoffRetry= new Retry(timeout, Retry.ExponentialBackOff(TimeSpan.FromMilliseconds(10)));
backoffRetry.Call(ContestedMethod);

var callTimes= new List<TimeSpan>();
var callTimer = new CallTimer(ts=> callTimes.Add(ts));
callTimer.Call(SleepAndReturn, 100);
callTimer.Call(SleepAndReturn,1000);
callTimes.First().ShouldBeLessThan( callTimes.Skip(1).First() );

``` 

Most InvokeWrapper constructors take optional callback parameters to assist in logging, e.g.:

```
CircuitBreaker(string circuitName, int? errorsBeforeBreaking = null, TimeSpan? breakForHowLong = null, 
        Type[] exceptionsToThrowNotBreak = null, 
        Type[] exceptionsToBreak = null, 
        Action<string> onBeforeInvoke=null, 
        Action<Exception> onExceptionWasCaught = null, 
        Action<Exception> onExceptionWillBeThrown = null,
        Action<string> onDroppedCallWhileCircuitBroken = null)
```

