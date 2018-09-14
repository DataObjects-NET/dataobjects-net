// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.03.12

using System;
using Xtensive.Orm.Logging;
using System.Threading.Tasks;


namespace Xtensive.Core
{
  internal sealed class AsyncFutureResult<T> : FutureResult<T>
  {
    private readonly BaseLog logger;
    private Task<T> task;

    public override bool IsAvailable
    {
      get { return task!=null; }
    }

    public override T Get()
    {
      if (!IsAvailable)
        throw new InvalidOperationException(Strings.ExResultIsNotAvailable);

      var localtTask = task;
      task = null;
      return localtTask.Result;
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

      this.logger = logger;

      task = Task.Run(worker);
    }
  }
}