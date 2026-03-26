// Copyright (C) 2014-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    private Func<Task<T>> worker;

    public override bool IsAvailable => task != null || worker != null;

    public override T Get()
    {
      if (!IsAvailable) {
        throw new InvalidOperationException(Strings.ExResultIsNotAvailable);
      }

      var localTask = task ?? worker();
      task = null;
      worker = null;
      return localTask.GetAwaiter().GetResult();
    }

    public override async ValueTask<T> GetAsync()
    {
      if (!IsAvailable) {
        throw new InvalidOperationException(Strings.ExResultIsNotAvailable);
      }

      var localTask = task ?? worker();
      task = null;
      worker = null;
      return await localTask.ConfigureAwait(false);
    }

    public override void Dispose()
    {
      if (!IsAvailable) {
        return;
      }

      try {
        Get();
      }
      catch (Exception exception) {
        logger?.Warning(nameof(Strings.LogAsyncOperationError), exception: exception);
      }
    }

    public override async ValueTask DisposeAsync()
    {
      if (!IsAvailable) {
        return;
      }

      try {
        await GetAsync().ConfigureAwait(false);
      }
      catch (Exception exception) {
        logger?.Warning(nameof(Strings.LogAsyncOperationError), exception: exception);
      }
    }

    // Constructors

    public AsyncFutureResult(Func<T> worker, BaseLog logger)
    {
      ArgumentValidator.EnsureArgumentNotNull(worker, nameof(worker));

      this.logger = logger;

      task = Task.Run(worker);
    }

    public AsyncFutureResult(Func<Task<T>> worker, BaseLog logger, bool startWorker)
    {
      ArgumentValidator.EnsureArgumentNotNull(worker, nameof(worker));

      this.logger = logger;

      if (startWorker) {
        task = Task.Run(worker);
      }
      else {
        this.worker = worker;
      }
    }
  }
}
