// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2009.10.12

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Rse.Providers;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;

namespace Xtensive.Orm.Rse.Transformation
{
  internal sealed class RedundantColumnRemover : ColumnMappingInspector
  {
    protected override Pair<CompilableProvider, List<int>> OverrideRightApplySource(ApplyProvider applyProvider, CompilableProvider provider, List<int> requestedMapping)
    {
      var currentMapping = mappings[applyProvider.Right];
      if (currentMapping.SequenceEqual(requestedMapping))
        return base.OverrideRightApplySource(applyProvider, provider, requestedMapping);
      var selectingRequestedMapping = requestedMapping.ToArray();
      for (int i = 0, count = requestedMapping.Count; i < count; i++) {
        selectingRequestedMapping[i] = currentMapping.IndexOf(selectingRequestedMapping[i]);
      }

      var selectProvider = new SelectProvider(provider, selectingRequestedMapping);
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

    private static Expression<Func<IEnumerable<Tuple>>> RemapRawProviderSource(Expression<Func<IEnumerable<Tuple>>> source, MapTransform mappingTransform)
    {
      var selectMethodInfo = typeof(Enumerable)
        .GetMethods()
        .Single(methodInfo => methodInfo.Name == "Select"
          && methodInfo.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>))
        .MakeGenericMethod(typeof(Tuple), typeof(Tuple));

      Func<Tuple, Tuple> selector = tuple => mappingTransform.Apply(TupleTransformType.Auto, tuple);
      var newExpression = Expression.Call(selectMethodInfo, source.Body, Expression.Constant(selector));
      return (Expression<Func<IEnumerable<Tuple>>>)FastExpression.Lambda(newExpression);
    }

    // Constructors

    public RedundantColumnRemover(CompilableProvider originalProvider)
      : base(originalProvider)
    {
    }
  }
}