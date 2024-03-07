// Copyright (C) 2014-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2014.03.12


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