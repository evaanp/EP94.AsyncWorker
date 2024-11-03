using EP94.AsyncWorker.Internal;
using EP94.AsyncWorker.Internal.Models;
using EP94.AsyncWorker.Internal.Utils;
using EP94.AsyncWorker.Public;
using EP94.AsyncWorker.Public.Interfaces;
using EP94.AsyncWorker.Public.Models;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection.Emit;
using System.Reflection;
using System.Threading;
using System.Threading.Channels;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);


IWorkFactory workFactory = IWorkFactory.Create(int.MaxValue, TaskScheduler.Current, TimeSpan.FromMilliseconds(1000));

var now = workFactory.CreateWork(() => DateTimeOffset.Now);
var tenSeconds = workFactory.CreateWork(() => DateTimeOffset.Now.AddSeconds(10));

int counter = 0;

var trigger = workFactory.CreateTimebasedTrigger<int>((c) =>
{
    return Task.FromResult(++counter);
}, now, tenSeconds);

trigger.ThenDo((i, c) =>
{
    Console.WriteLine(i);
    return Task.CompletedTask;
})
    .Subscribe();

//var stacktrace = Test22()();

//Console.WriteLine();

//Func<string, int> func = Test2;

//Console.WriteLine(func);

//int Test2(string value)
//{
//    return 5;
//}

//var work = workFactory.CreateWork(async (c) =>
//{
//    await Task.Delay(10);
//});
//StackFrameHelper helper = new StackFrameHelper();
//Thread.CurrentT

Func<object> Test22()
{
    var stackFrameHelperType = typeof(object).Assembly.GetType("System.Diagnostics.StackFrameHelper");

    var GetStackFramesInternal = Type.GetType("System.Diagnostics.StackTrace, mscorlib").GetMethod("GetStackFramesInternal", BindingFlags.Static | BindingFlags.NonPublic);



    var method = new DynamicMethod("GetStackTraceFast", typeof(object), new Type[0], typeof(StackTrace), true);


    var generator = method.GetILGenerator();

    generator.DeclareLocal(stackFrameHelperType);

    //generator.Emit(OpCodes.Ldc_I4_0);

    //generator.Emit(OpCodes.Call, typeof(Thread).GetProperty(nameof(Thread.CurrentThread), BindingFlags.Static | BindingFlags.Public).GetMethod);
    generator.Emit(OpCodes.Ldnull);

    generator.Emit(OpCodes.Newobj, stackFrameHelperType.GetConstructor(new[] { typeof(Thread) }));

    generator.Emit(OpCodes.Stloc_0);

    generator.Emit(OpCodes.Ldloc_0);

    generator.Emit(OpCodes.Ldc_I4_0);

    generator.Emit(OpCodes.Ldnull);

    generator.Emit(OpCodes.Call, GetStackFramesInternal);

    generator.Emit(OpCodes.Ldloc_0);

    generator.Emit(OpCodes.Ret);

    return (Func<object>)method.CreateDelegate(typeof(Func<object>));
}

//while (true)
//{
//    try
//    {
//        await work;
//    }
//    catch (Exception e)
//    {
//        Console.WriteLine(e);
//    }
//    await Task.Delay(1000);
//}
//await Test(workFactory);
//await workFactory.CreateWork(async (c) =>
//{
//    await Task.Delay(10);
//});
//Console.WriteLine();
//while (true)
//{
//    await Test(workFactory);
//    Console.WriteLine("Finish");
//    await Task.Delay(1000);
//}
//await Task.WhenAll(Enumerable.Range(0, 10).Select(x => Task.Run(() => Test(workFactory))));

async Task Test(IWorkFactory workFactory)
{
    List<int> results = new List<int>();
    int[] expectedValues = Enumerable.Range(0, 100).ToArray();
    List<IActionWorkHandle> workHandles = [];
    var cancelToken = new CancellationTokenSource(2000);
    for (int i = 0; i < expectedValues.Length; i++)
    {
        int index = i;
        IActionWorkHandle workHandle = workFactory.CreateWork(async (c) =>
        {
            await Task.Delay(10, c);
            results.Add(expectedValues[index]);
            //return Task.CompletedTask;
        }, cancelToken.Token)
            .ConfigureRetainResult(RetainResult.RetainLast);
        workHandles.Add(workHandle);
    }
    List<Task> tasks = new List<Task>();
    foreach (IActionWorkHandle workHandle in workHandles)
    {
        tasks.Add(workHandle.AsTask());
    }
    try
    {
        //await Task.WhenAll(tasks).WaitAsync(TimeSpan.FromSeconds(4));
        await Task.WhenAll(tasks);
    }
    catch (TimeoutException)
    {
        Task[] t = tasks.Where(x => !x.IsCompleted).ToArray();
        Console.WriteLine();
    }
}

//IWorkFactory workFactory = IWorkFactory.Create(1, defaultTimeout: TimeSpan.FromSeconds(1), cancellationToken: CancellationToken.None);


//var result = await workFactory.CreateWork(() => DateTimeOffset.UtcNow.AddDays(1).Date.AddMinutes(1));

//string str = result.ToLocalTime().ToLongTimeString();

//Console.WriteLine();

//var trigger = workFactory.CreateTimebasedTrigger<int>((c) =>
//{
//    Console.WriteLine("Hoi");
//    return Task.FromResult(5);
//}, workFactory.CreateWork(() => DateTimeOffset.UtcNow), workFactory.CreateWork(() => DateTimeOffset.UtcNow.AddSeconds(1)));


//trigger.Subscribe(x =>
//{
//    Console.WriteLine(x);
//});
//ITrigger<int> subject1 = workFactory.CreateTrigger<int>();
//ReplaySubject<B> subject2 = new ReplaySubject<B>(1);

//subject1.OnNext(10);
//subject2.OnNext(new AA());

//Task.Run(async () =>
//{
//    await Task.Delay(4000).ConfigureAwait(false);
//    Observable.CombineLatest(subject1, subject2, (a, b) => (a, b))
//        .Subscribe(x =>
//        {
//            Console.WriteLine(x);
//        });
//});




//CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

//var t2 = workFactory.CreateTrigger<bool>();

//var t = workFactory.CreateTimebasedTrigger((c) => Task.FromResult(true), workFactory.CreateWork(() => DateTimeOffset.Now), workFactory.CreateWork(() => DateTimeOffset.Now.AddSeconds(2)));

//t.ThenDo(x => t2.OnNext(x)).Subscribe();

//t2.Subscribe(x =>
//{
//    Console.WriteLine(x);
//}, cancellationTokenSource.Token);
//var result = await workFactory.CreateWork(() => DateTimeOffset.UtcNow.AddSeconds(5));

builder.Build().Run();

class AA() : B
{
    public int Value { get; set; } = 5;
};

interface B
{
    int Value { get; }
}
