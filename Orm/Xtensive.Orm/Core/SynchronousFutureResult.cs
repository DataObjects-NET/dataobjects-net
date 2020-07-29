// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.03.12

using System;
using System.Threading.Tasks;

namespace Xtensive.Core
{
  internal sealed class SynchronousFutureResult<T> : FutureResult<T>
  {
    private Func<T> worker;

    public override bool IsAvailable => worker!=null;

    public override T Get()
    {
      if (!IsAvailable) {
        throw new InvalidOperationException(Strings.ExResultIsNotAvailable);
      }

      var localWorker = worker;
      worker = null;
      return localWorker.Invoke();
    }

    public override ValueTask<T> GetAsync() => new ValueTask<T>(Get());

    public override void Dispose()
    { }

    public override ValueTask DisposeAsync() => default;


    // Constructors

    public SynchronousFutureResult(Func<T> worker)
    {
      ArgumentValidator.EnsureArgumentNotNull(worker, nameof(worker));

      this.worker = worker;
    }
  }
}