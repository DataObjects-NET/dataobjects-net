// Copyright (C) 2020-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm.Rse;

namespace Xtensive.Orm.Linq.Materialization
{
  internal interface IMaterializingReader<out TItem>
  {
    Session Session { get; }
    IEnumerator<TItem> AsEnumerator();
    IAsyncEnumerator<TItem> AsAsyncEnumerator();
  }

  internal class MaterializingReader<TItem>: IMaterializingReader<TItem>, IEnumerator<TItem>, IAsyncEnumerator<TItem>
  {
    private readonly RecordSetReader recordSetReader;
    private readonly MaterializationContext context;
    private readonly ParameterContext parameterContext;
    private readonly IItemMaterializer<TItem> itemMaterializer;
    private readonly Queue<Action> materializationQueue;

    public Session Session => context.Session;

    public IEnumerator<TItem> AsEnumerator()
    {
      recordSetReader.Reset();
      return this;
    }

    public IAsyncEnumerator<TItem> AsAsyncEnumerator()
    {
      recordSetReader.Reset();
      return this;
    }

    object IEnumerator.Current => Current;

    public TItem Current =>
      itemMaterializer.Materialize(recordSetReader.Current, context, parameterContext);

    public void Reset() => throw new NotSupportedException();

    public bool MoveNext()
    {
      while (recordSetReader.MoveNext()) {
        if (itemMaterializer.CanMaterialize(recordSetReader.Current)) {
          return true;
        }
      }

      if (materializationQueue != null) {
        while (materializationQueue.TryDequeue(out var materializeSelf)) {
          materializeSelf.Invoke();
        }
      }
      return false;
    }

    public async ValueTask<bool> MoveNextAsync()
    {
      while (await recordSetReader.MoveNextAsync().ConfigureAwaitFalse()) {
        if (itemMaterializer.CanMaterialize(recordSetReader.Current)) {
          return true;
        }
      }

      if (materializationQueue != null) {
        while (materializationQueue.TryDequeue(out var materializeSelf)) {
          materializeSelf.Invoke();
        }
      }
      return false;
    }

    public void Dispose() => recordSetReader.Dispose();

    public ValueTask DisposeAsync() => recordSetReader.DisposeAsync();

    internal MaterializingReader(RecordSetReader recordSetReader, MaterializationContext context,
      ParameterContext parameterContext, IItemMaterializer<TItem> itemMaterializer)
    {
      this.recordSetReader = recordSetReader;
      this.context = context;
      this.parameterContext = parameterContext;
      this.itemMaterializer = itemMaterializer;
      if (context.MaterializationQueue == null) {
        context.MaterializationQueue = materializationQueue = new Queue<Action>();
      }
      else {
        materializationQueue = null;
      }
    }
  }
}