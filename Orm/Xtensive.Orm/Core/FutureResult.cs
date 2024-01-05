// Copyright (C) 2012-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.03.15


namespace Xtensive.Core
{
  internal abstract class FutureResult<T> : IDisposable, IAsyncDisposable
  {
    public abstract bool IsAvailable { get; }

    public abstract T Get();
    public abstract ValueTask<T> GetAsync();

    public abstract void Dispose();
    public abstract ValueTask DisposeAsync();
  }

}