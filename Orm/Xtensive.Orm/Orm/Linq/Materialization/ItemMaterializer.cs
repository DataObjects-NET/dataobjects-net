using System;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Linq.Materialization
{
  internal class ItemMaterializer<TItem>
  {
    private readonly Func<Tuple, ItemMaterializationContext, TItem> materializationDelegate;
    private readonly bool isAggregate;

    public TItem Materialize(Tuple tuple, MaterializationContext context, ParameterContext parameterContext) =>
      materializationDelegate.Invoke(tuple, new ItemMaterializationContext(context, parameterContext));

    public bool CanMaterialize(Tuple tuple) =>
      !(isAggregate && (tuple.GetFieldState(0) & TupleFieldState.Null)==TupleFieldState.Null);

    public ItemMaterializer(
      Func<Tuple, ItemMaterializationContext, TItem> materializationDelegate, bool isAggregate = false)
    {
      this.materializationDelegate = materializationDelegate;
      this.isAggregate = isAggregate;
    }
  }
}