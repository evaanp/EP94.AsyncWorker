using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Interfaces
{
    internal interface IChannel<T>
    {
        IChannelListener<T> AddListener();
        IChannel<TNew> CreateLinked<TNew>();
    }
}
