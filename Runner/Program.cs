using EP94.AsyncWorker.Internal;
using EP94.AsyncWorker.Internal.Models;
using EP94.AsyncWorker.Internal.Utils;
using EP94.AsyncWorker.Public;
using EP94.AsyncWorker.Public.Interfaces;
using EP94.AsyncWorker.Public.Models;
using Microsoft.Extensions.Hosting;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Channels;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);


IWorkFactory workFactory = IWorkFactory.Create(int.MaxValue, TaskScheduler.Current, TimeSpan.FromMilliseconds(1));

var work = workFactory.CreateWork(async (c) =>
{
    await Task.Delay(10);
});

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
while (true)
{
    await Test(workFactory);
    Console.WriteLine("Finish");
    await Task.Delay(1000);
}
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
        }, $"Item_{index}", cancelToken.Token)
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
