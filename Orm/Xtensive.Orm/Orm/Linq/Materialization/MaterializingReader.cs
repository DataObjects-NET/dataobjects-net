using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm.Rse;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Linq.Materialization
{
  public interface IMaterializingReader<out TItem>
  {
    IEnumerator<TItem> AsEnumerator();
    IAsyncEnumerator<TItem> AsAsyncEnumerator(CancellationToken token);
  }

  public class MaterializingReader<TItem>: IMaterializingReader<TItem>, IEnumerator<TItem>, IAsyncEnumerator<TItem>
  {
    private readonly RecordSetReader recordSetReader;
    private readonly MaterializationContext context;
    private readonly ParameterContext parameterContext;
    private readonly Func<Tuple, ItemMaterializationContext, TItem> itemMaterializer;
    private readonly Queue<Action> materializationQueue;

    public IEnumerator<TItem> AsEnumerator()
    {
      recordSetReader.Reset();
      return this;
    }

    public IAsyncEnumerator<TItem> AsAsyncEnumerator(CancellationToken token)
    {
      recordSetReader.Reset();
      return this;
    }

    object IEnumerator.Current => Current;

    public TItem Current =>
      itemMaterializer.Invoke(recordSetReader.Current, new ItemMaterializationContext(context, parameterContext));

    public void Reset() => throw new NotSupportedException();

    public bool MoveNext()
    {
      if (recordSetReader.MoveNext()) {
        return true;
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
      if (await recordSetReader.MoveNextAsync()) {
        return true;
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
      ParameterContext parameterContext, Func<Tuple, ItemMaterializationContext, TItem> itemMaterializer)
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