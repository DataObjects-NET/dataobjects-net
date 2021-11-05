// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2009.10.12

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Reflection;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;

namespace Xtensive.Orm.Rse.Transformation
{
  internal sealed class RedundantColumnRemover : ColumnMappingInspector
  {
    private static readonly MethodInfo SelectMethodInfo = WellKnownTypes.Enumerable
      .GetMethods()
      .Single(methodInfo => methodInfo.Name == nameof(Enumerable.Select)
        && methodInfo.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == WellKnownTypes.FuncOfTArgTResultType)
      .MakeGenericMethod(WellKnownOrmTypes.Tuple, WellKnownOrmTypes.Tuple);

    protected override Pair<CompilableProvider, List<int>> OverrideRightApplySource(ApplyProvider applyProvider, CompilableProvider provider, List<int> requestedMapping)
    {
      var currentMapping = mappings[applyProvider.Right];
      if (currentMapping.SequenceEqual(requestedMapping))
        return base.OverrideRightApplySource(applyProvider, provider, requestedMapping);
      var selectProvider = new SelectProvider(provider, requestedMapping.ToArray());
      return new Pair<CompilableProvider, List<int>>(selectProvider, requestedMapping);
    }

    protected override Provider VisitRaw(RawProvider provider)
    {
      var mapping = mappings[provider];
      if (mapping.SequenceEqual(Enumerable.Range(0, provider.Header.Length)))
        return provider;
      var mappingTransform = new MapTransform(true, provider.Header.TupleDescriptor, mapping.ToArray());
      var newExpression = RemapRawProviderSource(provider.Source, mappingTransform);
      return new RawProvider(provider.Header.Select(mapping), newExpression);
    }

    private static Expression<Func<ParameterContext, IEnumerable<Tuple>>> RemapRawProviderSource(
      Expression<Func<ParameterContext, IEnumerable<Tuple>>> source, MapTransform mappingTransform)
    {
      Func<Tuple, Tuple> selector = tuple => mappingTransform.Apply(TupleTransformType.Auto, tuple);
      var newExpression = Expression.Call(SelectMethodInfo, source.Body, Expression.Constant(selector));
      return (Expression<Func<ParameterContext, IEnumerable<Tuple>>>)FastExpression.Lambda(newExpression, source.Parameters[0]);
    }

    // Constructors

    public RedundantColumnRemover(CompilableProvider originalProvider)
      : base(originalProvider)
    {
    }
  }
}