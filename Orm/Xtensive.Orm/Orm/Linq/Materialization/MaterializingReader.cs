using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm.Rse.Providers;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Linq.Materialization
{
  public interface IMaterializingReader<out TItem>: IEnumerator<TItem>, IAsyncEnumerator<TItem>
  {}

  public class MaterializingReader<TItem>: IMaterializingReader<TItem>
  {
    private readonly TupleReader tupleReader;
    private readonly MaterializationContext context;
    private readonly ParameterContext parameterContext;
    private readonly Func<Tuple, ItemMaterializationContext, TItem> itemMaterializer;
    private readonly Queue<Action> materializationQueue;

    object IEnumerator.Current => Current;

    public TItem Current =>
      itemMaterializer.Invoke(tupleReader.Current, new ItemMaterializationContext(context, parameterContext));

    public void Reset() => throw new NotSupportedException();

    public bool MoveNext()
    {
      if (tupleReader.MoveNext()) {
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
      if (await tupleReader.MoveNextAsync()) {
        return true;
      }

      if (materializationQueue != null) {
        while (materializationQueue.TryDequeue(out var materializeSelf)) {
          materializeSelf.Invoke();
        }
      }
      return false;
    }

    public void Dispose() => tupleReader.Dispose();

    public ValueTask DisposeAsync() => tupleReader.DisposeAsync();

    internal MaterializingReader(TupleReader tupleReader, MaterializationContext context,
      ParameterContext parameterContext, Func<Tuple, ItemMaterializationContext, TItem> itemMaterializer)
    {
      this.tupleReader = tupleReader;
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