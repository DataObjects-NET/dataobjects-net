// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.03.12

using System;
using Xtensive.Orm.Logging;
#if NETSTANDARD
using System.Threading.Tasks;
#endif


namespace Xtensive.Core
{
  internal sealed class AsyncFutureResult<T> : FutureResult<T>
  {
    private readonly BaseLog logger;
    private Func<T> worker;
    private IAsyncResult asyncResult;
#if NETSTANDARD
    private Task<T> asyncTask;
#endif


    public override bool IsAvailable
    {
      get { return worker!=null; }
    }

    public override T Get()
    {
      if (!IsAvailable)
        throw new InvalidOperationException(Strings.ExResultIsNotAvailable);

#if NETSTANDARD
        asyncTask.Wait();
        var localResult = asyncTask.Result;
        asyncResult = null;
        worker = null;
        return localResult;
#else
        var localWorker = worker;
        var localAsyncResult = asyncResult;
        asyncResult = null;
        worker = null;
        return localWorker.EndInvoke(localAsyncResult);
#endif
    }

    public override void Dispose()
    {
      if (!IsAvailable)
        return;

      try {
        Get();
      }
      catch (Exception exception) {
        if (logger!=null)
          logger.Warning(Strings.LogAsyncOperationError, exception: exception);
      }
    }

    // Constructors

    public AsyncFutureResult(Func<T> worker, BaseLog logger)
    {
      ArgumentValidator.EnsureArgumentNotNull(worker, "worker");

      this.worker = worker;
      this.logger = logger;
#if NETSTANDARD
      asyncTask = Task.Run(worker);
#else
      asyncResult = worker.BeginInvoke(null, null);
#endif
    }
  }
}