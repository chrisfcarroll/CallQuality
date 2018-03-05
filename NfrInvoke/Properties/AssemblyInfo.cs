using System.Reflection;

[assembly:AssemblyDescription(
@"NFRInvoke : a simple pattern for NFRs for function calls :retry, circuit-breaker, caching, timing, logging, etc.
-------------------------------------------------------------------------------------------------------

Apply NFRs & cross-cutting concerns in function calls, when you cannot or do not wish to use MSIL-rewrite techniques.

Examples
```
var cacher = new Cacher(TimeSpan.FromSeconds(99));
var r1=cacher.Call(ExpensiveMethod);
var r2=cacher.Call(ExpensiveMethod);

var breaker= new CircuitBreaker(""Test1"",1,TimeSpan.FromSeconds(1));
var r3=breaker.Call(Method1, p1,p2); 
var r4=breaker.Call(Method2, p1,p2,p3);

var r5= new Retry(1,1).Do(Method3, 1)

var callTimes= new List<TimeSpan>();
var callTimer = new CallTimer(ts=> callTimes.Add(ts));
callTimer.Call(SleepAndReturn, 100);
callTimer.Call(SleepAndReturn,1000);
callTimes.First().ShouldBeLessThan( callTimes.Skip(1).First() );

``` 

<PackageVersion>2.0.0.0-prerelease-pending-system.runtime.caching</PackageVersion>
<AssemblyFileVersion>2.0.0.0</AssemblyFileVersion>
<AssemblyVersion>2.0.0.0</AssemblyVersion>
<Title>NFRInvoke - a simple extensible pattern for wrapping function calls for NFRs :retry, circuit-breaker, caching, timing, logging, etc. </Title>
<AssemblyDescription></AssemblyDescription>
<ReleaseNotes>2.0 netstandard 2</ReleaseNotes>
<Copyright>(c) Chris F. Carroll, 2016-2018</Copyright>
<Authors>Chris F Carroll</Authors>
<Owners>Chris F Carroll</Owners>
<ProjectUrl>http://github.com/chrisfcarroll/InvokeWrappers</ProjectUrl>
<RequireLicenseAcceptance>false</RequireLicenseAcceptance>
<Tags>caching timer circuit-breaker circuitbreaker retry logging nfr architectural invoke</Tags>
")]