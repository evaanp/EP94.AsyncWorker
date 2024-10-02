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


var trigger = workFactory.CreateTriggerAsync<int>(false, "Trigger");


builder.Build().Run();