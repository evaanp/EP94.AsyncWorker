using EP94.AsyncWorker.Internal;
using EP94.AsyncWorker.Internal.Models;
using EP94.AsyncWorker.Public;
using EP94.AsyncWorker.Public.Interfaces;
using EP94.AsyncWorker.Public.Models;
using Microsoft.Extensions.Hosting;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Threading.Channels;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

IWorkFactory workFactory = IWorkFactory.Create(5, cancellationToken: CancellationToken.None);


var trigger = workFactory.CreateTriggerAsync<int>("Trigger");



//await Task.Delay(5000);

var workHandle = trigger.Then((i, c) =>
{
    return Task.FromResult(i);
}, name: "Then");
trigger.OnNext(25);

await Task.Delay(5000);
workHandle.Subscribe(x =>
{
    Console.WriteLine(x);
});
//channel.Writer.Complete(new Exception("hoi"));

//var result2 = channel.Reader.ReadAllAsync().ToBlockingEnumerable().ToArray();
//channel.
//var result = await workFactory.CreateWork((CancellationToken c) =>
//{
//    return Task.FromResult(5);
//});
Console.WriteLine();

//bool failed = false;

//await workFactory.CreateWork((c) =>
//{
//    if (!failed)
//    {
//        failed = true;
//        throw new Exception();
//    }
//    Console.WriteLine("Jeej");
//    return Task.CompletedTask;
//}, options => options.RetryCount = 2);

//IWorkHandle<int> workHandle2 = workFactory.CreateWork((c) =>
//{
//    Console.WriteLine("Work1");
//    return Task.FromResult(5);
//})
//.Then((p, c) =>
//{
//    Console.WriteLine(p);
//    return Task.FromResult(p);
//});

//Random random = new Random();
//IBackgroundWork<int> backgroundWork = workFactory.CreateBackgroundWork(c => Task.FromResult(random.Next()), TimeSpan.FromSeconds(2));

//IWorkHandle then1 = backgroundWork.Then((a, c) =>
//{
//    Console.WriteLine("Then 1 : " + a);
//    return Task.CompletedTask;
//});

//IWorkHandle then2 = backgroundWork.Then((a, c) =>
//{
//    Console.WriteLine("Then 2 : " + a);
//    return Task.CompletedTask;
//});


//backgroundWork.Subscribe(x =>
//{
//    Console.WriteLine(x);
//});

//ITrigger<int> trigger = worker.CreateTriggerAsync<int>(WorkDelegate.Create((int i, CancellationToken c) =>
//{
//    Console.WriteLine(i);
//    return Task.CompletedTask;
//}));


//Timer timer = new Timer(async (a) =>
//{
//    await trigger.OnNextAsync(25);
//}, null, 0, 1000);



//worker.CreateTriggerAsync<int>((int i, CancellationToken c) => Task.CompletedTask);

//worker.ScheduleBackgroundWorkAsync<double>((c) =>
//{
//    return Task.FromResult(random.NextDouble());
//}, TimeSpan.FromSeconds(1), () => true)
//    .Subscribe(x =>
//    {
//        Console.WriteLine(x);
//    });

//IWorkHandle<string> unitOfWork = worker.ScheduleWorkAsync(async () =>
//{
//    //await Task.Delay(5000);
//    Console.WriteLine("Jeeej");
//    throw new Exception();
//    return "hoi";
//}, 10);
//IWorkHandle<string> unitOfWork1 = worker.ScheduleWorkAsync(async () =>
//{
//    await Task.Delay(6000);
//    Console.WriteLine("Jeeej");
//    return "hoi";
//});
//IWorkHandle<string> unitOfWork2 = worker.ScheduleWorkAsync(async () =>
//{
//    await Task.Delay(7000);
//    Console.WriteLine("Jeeej");
//    return "hoi";
//});
//IWorkHandle<string> unitOfWork3 = worker.ScheduleWorkAsync(async () =>
//{
//    await Task.Delay(8000);
//    Console.WriteLine("Jeeej");
//    return "hoi";
//});
//IWorkHandle<string> unitOfWork4 = worker.ScheduleWorkAsync(async () =>
//{
//    await Task.Delay(9000);
//    Console.WriteLine("Jeeej");
//    return "hoi";
//});

//string result = await unitOfWork;


builder.Build().Run();