// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.03.12

using System;
using Xtensive.Orm.Logging;

namespace Xtensive.Core
{
  internal sealed class AsyncFutureResult<T> : FutureResult<T>
  {
    private readonly BaseLog logger;
    private Func<T> worker;
    private IAsyncResult asyncResult;

    public override bool IsAvailable
    {
      get { return worker!=null; }
    }

    public override T Get()
    {
      if (!IsAvailable)
        throw new InvalidOperationException(Strings.ExResultIsNotAvailable);

      var localWorker = worker;
      var localAsyncResult = asyncResult;
      asyncResult = null;
      worker = null;
      return localWorker.EndInvoke(localAsyncResult);
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

      asyncResult = worker.BeginInvoke(null, null);
    }
  }
}