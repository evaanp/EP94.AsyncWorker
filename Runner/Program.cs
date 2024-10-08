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

IWorkFactory workFactory = IWorkFactory.Create(1, defaultTimeout: TimeSpan.FromSeconds(1), cancellationToken: CancellationToken.None);



builder.Build().Run();