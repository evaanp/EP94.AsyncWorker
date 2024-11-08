﻿using EP94.AsyncWorker.Internal.Interfaces;
using EP94.AsyncWorker.Public.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Models
{
    internal class UnwrapProxy2<TInnerResult> : ObservableWorkBase<TInnerResult>, IObservable<TInnerResult>
    {
        protected override IObservable<TInnerResult> RunObservable { get; }
        private ISubject<IResultWorkHandle<IObservable<TInnerResult>>> _parameterSubject;

        public UnwrapProxy2(IObservable<IObservable<TInnerResult>> workHandle, CancellationToken cancellationToken)
            : base (cancellationToken)
        {
            //_parameterSubject = new BehaviorSubject<IResultWorkHandle<IObservable<TInnerResult>>>(workHandle);
            RunObservable = workHandle
                //.Select(x => x.Switch())
                .Switch();

            //_parameterSubject.OnNext(workHandle);
        }
    }
}
