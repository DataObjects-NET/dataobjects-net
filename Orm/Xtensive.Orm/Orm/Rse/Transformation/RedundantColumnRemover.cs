// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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