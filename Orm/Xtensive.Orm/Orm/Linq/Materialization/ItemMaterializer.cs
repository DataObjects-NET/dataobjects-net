// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Core;
using Xtensive.Orm.Rse;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Linq.Materialization
{
  internal interface IItemMaterializer<TItem>
  {
    TItem Materialize(Tuple tuple, MaterializationContext context, ParameterContext parameterContext);
    bool CanMaterialize(Tuple tuple);
  }

  internal class ItemMaterializer<TItem> : IItemMaterializer<TItem>
  {
    private readonly Func<Tuple, ItemMaterializationContext, TItem> materializationDelegate;

    public virtual TItem Materialize(Tuple tuple, MaterializationContext context, ParameterContext parameterContext) =>
      materializationDelegate.Invoke(tuple, new ItemMaterializationContext(context, parameterContext));

    public virtual bool CanMaterialize(Tuple tuple) => true;

    public ItemMaterializer(Func<Tuple, ItemMaterializationContext, TItem> materializationDelegate)
    {
      this.materializationDelegate = materializationDelegate;
    }
  }

  internal class AggregateResultMaterializer<TResult> : ItemMaterializer<TResult>
  {
    private readonly AggregateType aggregateType;

    public override bool CanMaterialize(Tuple tuple) =>
      aggregateType==AggregateType.Sum || (tuple.GetFieldState(0) & TupleFieldState.Null) == 0;

    public AggregateResultMaterializer(
      Func<Tuple, ItemMaterializationContext, TResult> materializationDelegate, AggregateType aggregateType)
      : base(materializationDelegate)
    {
      this.aggregateType = aggregateType;
    }
  }

  internal class NullableAggregateResultMaterializer<TResult> : ItemMaterializer<TResult?>
    where TResult: struct
  {
    private readonly AggregateType aggregateType;

    public override TResult? Materialize(Tuple tuple, MaterializationContext context, ParameterContext parameterContext)
    {
      var result = base.Materialize(tuple, context, parameterContext);
      return aggregateType == AggregateType.Sum ? result ?? default : result;
    }

    public NullableAggregateResultMaterializer(
      Func<Tuple, ItemMaterializationContext, TResult?> materializationDelegate, AggregateType aggregateType)
      : base(materializationDelegate)
    {
      this.aggregateType = aggregateType;
    }
  }
}