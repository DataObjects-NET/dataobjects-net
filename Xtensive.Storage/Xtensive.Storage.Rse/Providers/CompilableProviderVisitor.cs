// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.10

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Rse.Providers.Compilable;
using System.Linq;

namespace Xtensive.Storage.Rse.Providers
{
  [Serializable]
  public abstract class CompilableProviderVisitor : ProviderVisitor
  {
    protected readonly Func<Expression, Expression> translate;

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
      var source = VisitCompilable(provider.Source);
      if (source == provider.Source)
        return provider;
      return new ExecutionSiteProvider(source, provider.Options, provider.Location);
    }

    /// <inheritdoc/>
    protected override Provider VisitTake(TakeProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      var count = translate(provider.Count);
      if (source == provider.Source && count == provider.Count)
        return provider;
      return new TakeProvider(source, (Expression<Func<int>>) count);
    }

    /// <inheritdoc/>
    protected override Provider VisitSkip(SkipProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      var count = translate(provider.Count);
      if (source == provider.Source && count == provider.Count)
        return provider;
      return new SkipProvider(source, (Expression<Func<int>>)count);
    }

    /// <inheritdoc/>
    protected override Provider VisitSelect(SelectProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      if (source == provider.Source)
        return provider;
      return new SelectProvider(source, provider.ColumnIndexes);
    }

    /// <inheritdoc/>
    protected override Provider VisitSeek(SeekProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      var key = translate(provider.Key);
      if (source == provider.Source && key == provider.Key)
        return provider;
      return new SeekProvider(source, (Expression<Func<Tuple>>) key);
    }

    /// <inheritdoc/>
    protected override Provider VisitRaw(RawProvider provider)
    {
      var source = translate(provider.Source);
      if (source == provider.Source)
        return provider;
      return new RawProvider(provider.Header, (Expression<Func<Tuple[]>>) source);
    }

    /// <inheritdoc/>
    protected override Provider VisitRange(RangeProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      var range = translate(provider.Range);
      if (source == provider.Source && range == provider.Range)
        return provider;
      return new RangeProvider(source, (Expression<Func<Range<Entire<Tuple>>>>) range);
    }

    /// <inheritdoc/>
    protected override Provider VisitSort(SortProvider provider)
    {
      // TODO: rewrite Order!!
      var source = VisitCompilable(provider.Source);
      if (source == provider.Source)
        return provider;
      return new SortProvider(source, provider.Order);
    }

    /// <inheritdoc/>
    protected override Provider VisitJoin(JoinProvider provider)
    {
      // TODO: we should not reference columns by its indexes
      var left = VisitCompilable(provider.Left);
      var right = VisitCompilable(provider.Right);
      if (left == provider.Left && right == provider.Right)
        return provider;
      return new JoinProvider(left, right, provider.LeftJoin, provider.JoinType, provider.EqualIndexes);
    }

    /// <inheritdoc/>
    protected override Provider VisitFilter(FilterProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      var predicate = translate(provider.Predicate);
      if (source == provider.Source && predicate == provider.Predicate)
        return provider;
      return new FilterProvider(source, (Expression<Func<Tuple, bool>>) predicate);
    }

    /// <inheritdoc/>
    protected override Provider VisitDistinct(DistinctProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      if (source == provider.Source)
        return provider;
      return new DistinctProvider(source);
    }

    /// <inheritdoc/>
    protected override Provider VisitCalculate(CalculationProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      var translated = false;
      var descriptors = new List<CalculatedColumnDescriptor>(provider.CalculatedColumns.Length);
      foreach (var column in provider.CalculatedColumns) {
        var expression = translate(column.Expression);
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
      var source = VisitCompilable(provider.Source);
      if (source == provider.Source)
        return provider;
      return new AliasProvider(provider, provider.Alias);
    }

    /// <inheritdoc/>
    protected override Provider VisitAggregate(AggregateProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      if (source == provider.Source)
        return provider;
      var acd = new List<AggregateColumnDescriptor>(provider.AggregateColumns.Length);
      acd.AddRange(provider.AggregateColumns.Select(ac => new AggregateColumnDescriptor(ac.Name, ac.Index, ac.AggregateType)));
      return new AggregateProvider(source, provider.GroupColumnIndexes, acd.ToArray());
     
    }

    /// <inheritdoc/>
    protected override Provider VisitStore(StoredProvider provider)
    {
      var compilableSource = provider.Source as CompilableProvider;
      if (compilableSource == null)
        return provider;
      var source = VisitCompilable(compilableSource);
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
      var source = VisitCompilable(provider.Source);
      if (source == provider.Source)
        return provider;
      return new ReindexProvider(source, provider.Order);
    }

    private static Expression DefaultExpressionTranslator(Expression e)
    {
      return e;
    }

    // Constructors

    /// <inheritdoc/>
    protected CompilableProviderVisitor()
      : this(DefaultExpressionTranslator)
    {
    }

    /// <inheritdoc/>
    /// <param name="expressionTranslator">Expression translator.</param>
    protected CompilableProviderVisitor(Func<Expression, Expression> expressionTranslator)
    {
      this.translate = expressionTranslator;
    }
  }
}