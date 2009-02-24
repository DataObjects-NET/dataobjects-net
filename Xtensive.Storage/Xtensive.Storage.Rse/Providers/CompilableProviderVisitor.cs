// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.10

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Rse.Providers.Compilable;
using System.Linq;

namespace Xtensive.Storage.Rse.Providers
{
  /// <summary>
  /// Abstract <see cref="CompilableProvider"/> visitor class. Result is <see cref="CompilableProvider"/>.
  /// </summary>
  [Serializable]
  public abstract class CompilableProviderVisitor : ProviderVisitor
  {
    protected Func<Provider, Expression, Expression> translate;

    /// <summary>
    /// Visits the compilable provider.
    /// </summary>
    /// <param name="cp">The compilable provider.</param>
    protected CompilableProvider VisitCompilable(CompilableProvider cp)
    {
      var result = (CompilableProvider)Visit(cp);
      return result;
    }

    /// <inheritdoc/>
    protected override Provider VisitExecutionSite(ExecutionSiteProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      OnRecursionExit(provider);
      if (source == provider.Source)
        return provider;
      return new ExecutionSiteProvider(source, provider.Options, provider.Location);
    }

    /// <inheritdoc/>
    protected override Provider VisitTake(TakeProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      OnRecursionExit(provider);
      var count = translate(provider, provider.Count);
      if (source == provider.Source && count == provider.Count)
        return provider;
      return new TakeProvider(source, (Expression<Func<int>>) count);
    }

    /// <inheritdoc/>
    protected override Provider VisitSkip(SkipProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      OnRecursionExit(provider);
      var count = translate(provider, provider.Count);
      if (source == provider.Source && count == provider.Count)
        return provider;
      return new SkipProvider(source, (Expression<Func<int>>)count);
    }

    /// <inheritdoc/>
    protected override Provider VisitSelect(SelectProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      OnRecursionExit(provider);
      if (source == provider.Source)
        return provider;
      return new SelectProvider(source, provider.ColumnIndexes);
    }

    /// <inheritdoc/>
    protected override Provider VisitSeek(SeekProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      OnRecursionExit(provider);
      var key = translate(provider, provider.Key);
      if (source == provider.Source && key == provider.Key)
        return provider;
      return new SeekProvider(source, (Expression<Func<Tuple>>) key);
    }

    /// <inheritdoc/>
    protected override Provider VisitRaw(RawProvider provider)
    {
      var source = translate(provider, provider.Source);
      if (source == provider.Source)
        return provider;
      return new RawProvider(provider.Header, (Expression<Func<Tuple[]>>) source);
    }

    /// <inheritdoc/>
    protected override Provider VisitRange(RangeProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      OnRecursionExit(provider);
      var range = translate(provider, provider.Range);
      if (source == provider.Source && range == provider.Range)
        return provider;
      return new RangeProvider(source, (Expression<Func<Range<Entire<Tuple>>>>) range);
    }

    /// <inheritdoc/>
    protected override Provider VisitSort(SortProvider provider)
    {
      // TODO: rewrite Order!!
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      OnRecursionExit(provider);
      if (source == provider.Source)
        return provider;
      return new SortProvider(source, provider.Order);
    }

    /// <inheritdoc/>
    protected override Provider VisitJoin(JoinProvider provider)
    {
      // TODO: we should not reference columns by its indexes
      OnRecursionEntrance(provider);
      var left = VisitCompilable(provider.Left);
      var right = VisitCompilable(provider.Right);
      var equalIndexes = OnRecursionExit(provider);
      if (left == provider.Left && right == provider.Right)
        return provider;
      return new JoinProvider(left, right, provider.LeftJoin, provider.JoinType,
        equalIndexes != null ? (Pair<int>[])equalIndexes : provider.EqualIndexes);
    }

    /// <inheritdoc/>
    protected override Provider VisitFilter(FilterProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      OnRecursionExit(provider);
      var predicate = translate(provider, provider.Predicate);
      if (source == provider.Source && predicate == provider.Predicate)
        return provider;
      return new FilterProvider(source, (Expression<Func<Tuple, bool>>) predicate);
    }

    /// <inheritdoc/>
    protected override Provider VisitDistinct(DistinctProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      OnRecursionExit(provider);
      if (source == provider.Source)
        return provider;
      return new DistinctProvider(source);
    }

    /// <inheritdoc/>
    protected override Provider VisitCalculate(CalculationProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      OnRecursionExit(provider);
      var translated = false;
      var descriptors = new List<CalculatedColumnDescriptor>(provider.CalculatedColumns.Length);
      foreach (var column in provider.CalculatedColumns) {
        var expression = translate(provider, column.Expression);
        if (expression != column.Expression)
          translated = true;
        var ccd = new CalculatedColumnDescriptor(column.Name, column.Type, (Expression<Func<Tuple, object>>) expression);
        descriptors.Add(ccd);
      }
      if (!translated && source == provider.Source)
        return provider;
      return new CalculationProvider(source, descriptors.ToArray());
    }

    /// <inheritdoc/>
    protected override Provider VisitAlias(AliasProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      OnRecursionExit(provider);
      if (source == provider.Source)
        return provider;
      return new AliasProvider(source, provider.Alias);
    }

    /// <inheritdoc/>
    protected override Provider VisitAggregate(AggregateProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      var resultParameters = OnRecursionExit(provider);
      if (source == provider.Source)
        return provider;
      if (resultParameters == null) {
        var acd = new List<AggregateColumnDescriptor>(provider.AggregateColumns.Length);
        acd.AddRange(provider.AggregateColumns.Select(ac => new AggregateColumnDescriptor(ac.Name, ac.Index, ac.AggregateType)));
        return new AggregateProvider(source, provider.GroupColumnIndexes, acd.ToArray());
      }
      var result = (Pair<int[], AggregateColumnDescriptor[]>) resultParameters;
      return new AggregateProvider(source, result.First, result.Second);
    }

    /// <inheritdoc/>
    protected override Provider VisitStore(StoredProvider provider)
    {
      var compilableSource = provider.Source as CompilableProvider;
      if (compilableSource == null)
        return provider;
      OnRecursionEntrance(provider);
      var source = VisitCompilable(compilableSource);
      OnRecursionExit(provider);
      if (source == compilableSource)
        return provider;
      return new StoredProvider(source, provider.Scope, provider.Name);
    }

    /// <inheritdoc/>
    protected override Provider VisitIndex(IndexProvider provider)
    {
      return provider;
    }

    /// <inheritdoc/>
    protected override Provider VisitReindex(ReindexProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      OnRecursionExit(provider);
      if (source == provider.Source)
        return provider;
      return new ReindexProvider(source, provider.Order);
    }

    private static Expression DefaultExpressionTranslator(Provider p, Expression e)
    {
      return e;
    }

    /// <summary>
    /// Called after recursion exit.
    /// </summary>
    protected virtual object OnRecursionExit(Provider provider)
    {
      return null;
    }

    /// <summary>
    /// Called before recursion entrance.
    /// </summary>
    protected virtual void OnRecursionEntrance(Provider provider)
    {
    }

    // Constructors

    /// <inheritdoc/>
    protected CompilableProviderVisitor()
      : this(DefaultExpressionTranslator)
    {
    }

    /// <inheritdoc/>
    /// <param name="expressionTranslator">Expression translator.</param>
    protected CompilableProviderVisitor(Func<Provider, Expression, Expression> expressionTranslator)
    {
      translate = expressionTranslator;
    }
  }
}