// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.10

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Collections;
using Xtensive.Core;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// <see cref="CompilableProvider"/> visitor class. Result is <see cref="CompilableProvider"/>.
  /// </summary>
  [Serializable]
  public class CompilableProviderVisitor : ProviderVisitor<CompilableProvider>
  {
    protected Func<CompilableProvider, Expression, Expression> translate;

    /// <summary>
    /// Visits the compilable provider.
    /// </summary>
    /// <param name="cp">The compilable provider.</param>
    public CompilableProvider VisitCompilable(CompilableProvider cp) => Visit(cp);

    /// <inheritdoc/>
    protected override CompilableProvider VisitTake(TakeProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      _ = OnRecursionExit(provider);
      return source == provider.Source ? provider : new TakeProvider(source, provider.Count);
    }

    /// <inheritdoc/>
    protected override CompilableProvider VisitSkip(SkipProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      _ = OnRecursionExit(provider);
      return source == provider.Source ? provider : new SkipProvider(source, provider.Count);
    }

    /// <inheritdoc/>
    protected override CompilableProvider VisitPaging(PagingProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      _ = OnRecursionExit(provider);
      return source == provider.Source ? provider : new PagingProvider(source, provider);
    }

    /// <inheritdoc/>
    protected override CompilableProvider VisitSelect(SelectProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      var columnIndexes = (int[])OnRecursionExit(provider);
      return source == provider.Source
        ? provider
        : new SelectProvider(source, columnIndexes ?? provider.ColumnIndexes);
    }

    /// <inheritdoc/>
    protected override CompilableProvider VisitTag(TagProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      _ = OnRecursionExit(provider);
      return source == provider.Source ? provider : new TagProvider(source, provider.Tag);
    }

    /// <inheritdoc/>
    protected override CompilableProvider VisitSeek(SeekProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      _ = OnRecursionExit(provider);
      return source == provider.Source ? provider : new SeekProvider(source, provider.Key);
    }

    /// <inheritdoc/>
    protected override CompilableProvider VisitRaw(RawProvider provider)
    {
      return provider;
    }

    /// <inheritdoc/>
    protected override CompilableProvider VisitSort(SortProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      var order = OnRecursionExit(provider);
      return source == provider.Source
        ? provider
        : new SortProvider(source, (order == null) ? provider.Order : (DirectionCollection<int>)order);
    }

    /// <inheritdoc/>
    protected override CompilableProvider VisitJoin(JoinProvider provider)
    {
      OnRecursionEntrance(provider);
      var left = VisitCompilable(provider.Left);
      var right = VisitCompilable(provider.Right);
      var equalIndexes = OnRecursionExit(provider);
      return left == provider.Left && right == provider.Right
        ? provider
        : new JoinProvider(left, right, provider.JoinType,
            equalIndexes != null ? (Pair<int>[])equalIndexes : provider.EqualIndexes);
    }

    /// <inheritdoc/>
    protected override CompilableProvider VisitFilter(FilterProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      _ = OnRecursionExit(provider);
      var predicate = translate(provider, provider.Predicate);
      return source == provider.Source && predicate == provider.Predicate
        ? provider
        : new FilterProvider(source, (Expression<Func<Tuple, bool>>) predicate);
    }

    /// <inheritdoc/>
    protected override CompilableProvider VisitDistinct(DistinctProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      _ = OnRecursionExit(provider);
      return source == provider.Source ? provider : new DistinctProvider(source);
    }

    /// <inheritdoc/>
    protected override CompilableProvider VisitCalculate(CalculateProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      _ = OnRecursionExit(provider);
      var translated = false;
      var descriptors = new List<CalculatedColumnDescriptor>(provider.CalculatedColumns.Length);
      foreach (var column in provider.CalculatedColumns) {
        var expression = translate(provider, column.Expression);
        if (expression != column.Expression)
          translated = true;
        var ccd = new CalculatedColumnDescriptor(column.Name, column.Type, (Expression<Func<Tuple, object>>) expression);
        descriptors.Add(ccd);
      }
      return !translated && source == provider.Source
        ? provider
        : new CalculateProvider(source, descriptors.ToArray());
    }

    /// <inheritdoc/>
    protected override CompilableProvider VisitRowNumber(RowNumberProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      _ = OnRecursionExit(provider);
      return source == provider.Source ? provider : new RowNumberProvider(source, provider.SystemColumn.Name);
    }


    /// <inheritdoc/>
    protected override CompilableProvider VisitAlias(AliasProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      _ = OnRecursionExit(provider);
      return source == provider.Source ? provider : new AliasProvider(source, provider.Alias);
    }

    /// <inheritdoc/>
    protected override CompilableProvider VisitAggregate(AggregateProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      var resultParameters = OnRecursionExit(provider);
      if (source == provider.Source)
        return provider;
      if (resultParameters == null) {
        var acd = new List<AggregateColumnDescriptor>(provider.AggregateColumns.Length);
        acd.AddRange(provider.AggregateColumns.Select(ac => new AggregateColumnDescriptor(ac.Name, ac.SourceIndex, ac.AggregateType)));
        return new AggregateProvider(source, provider.GroupColumnIndexes, acd);
      }
      var result = (Pair<int[], AggregateColumnDescriptor[]>) resultParameters;
      return new AggregateProvider(source, result.First, (IReadOnlyList<AggregateColumnDescriptor>) result.Second);
    }

    /// <inheritdoc/>
    protected override CompilableProvider VisitStore(StoreProvider provider)
    {
      var compilableSource = provider.Source;
      OnRecursionEntrance(provider);
      var source = VisitCompilable(compilableSource);
      _ = OnRecursionExit(provider);
      return source == compilableSource ? provider : new StoreProvider(source, provider.Name);
    }

    /// <inheritdoc/>
    protected override CompilableProvider VisitIndex(IndexProvider provider)
    {
      OnRecursionEntrance(provider);
      _ = OnRecursionExit(provider);
      return provider;
    }

    /// <inheritdoc/>
    protected override CompilableProvider VisitFreeText(FreeTextProvider provider)
    {
      OnRecursionEntrance(provider);
      _ = OnRecursionExit(provider);
      return provider;
    }

    /// <inheritdoc/>
    protected override CompilableProvider VisitContainsTable(ContainsTableProvider provider)
    {
      OnRecursionEntrance(provider);
      _ = OnRecursionExit(provider);
      return provider;
    }

    /// <inheritdoc/>
    protected override CompilableProvider VisitPredicateJoin(PredicateJoinProvider provider)
    {
      OnRecursionEntrance(provider);
      var left = VisitCompilable(provider.Left);
      var right = VisitCompilable(provider.Right);
      var predicate = (Expression<Func<Tuple, Tuple, bool>>)OnRecursionExit(provider);
      return left == provider.Left && right == provider.Right
        ? provider
        : new PredicateJoinProvider(left, right, predicate ?? provider.Predicate, provider.JoinType);
    }

    /// <inheritdoc/>
    protected override CompilableProvider VisitExistence(ExistenceProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      _ = OnRecursionExit(provider);
      return source == provider.Source ? provider : new ExistenceProvider(source, provider.ExistenceColumnName);
    }

    /// <inheritdoc/>
    protected override CompilableProvider VisitApply(ApplyProvider provider)
    {
      OnRecursionEntrance(provider);
      var left = VisitCompilable(provider.Left);
      var right = VisitCompilable(provider.Right);
      _ = OnRecursionExit(provider);
      return left == provider.Left && right == provider.Right
        ? provider
        : new ApplyProvider(provider.ApplyParameter, left, right,
            provider.IsInlined, provider.SequenceType, provider.ApplyType);
    }

    /// <inheritdoc/>
    protected override CompilableProvider VisitIntersect(IntersectProvider provider)
    {
      OnRecursionEntrance(provider);
      var left = VisitCompilable(provider.Left);
      var right = VisitCompilable(provider.Right);
      _ = OnRecursionExit(provider);
      return left == provider.Left && right == provider.Right
        ? provider
        : new IntersectProvider(left, right);
    }

    /// <inheritdoc/>
    protected override CompilableProvider VisitExcept(ExceptProvider provider)
    {
      OnRecursionEntrance(provider);
      var left = VisitCompilable(provider.Left);
      var right = VisitCompilable(provider.Right);
      _ = OnRecursionExit(provider);
      return left == provider.Left && right == provider.Right
        ? provider
        : new ExceptProvider(left, right);
    }

    /// <inheritdoc/>
    protected override CompilableProvider VisitConcat(ConcatProvider provider)
    {
      OnRecursionEntrance(provider);
      var left = VisitCompilable(provider.Left);
      var right = VisitCompilable(provider.Right);
      _ = OnRecursionExit(provider);
      return left == provider.Left && right == provider.Right
        ? provider
        : new ConcatProvider(left, right);
    }

    /// <inheritdoc/>
    protected override CompilableProvider VisitUnion(UnionProvider provider)
    {
      OnRecursionEntrance(provider);
      var left = VisitCompilable(provider.Left);
      var right = VisitCompilable(provider.Right);
      _ = OnRecursionExit(provider);
      return left == provider.Left && right == provider.Right
        ? provider
        : new UnionProvider(left, right);
    }

    /// <inheritdoc/>
    protected override CompilableProvider VisitLock(LockProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      _ = OnRecursionExit(provider);
      return source == provider.Source ? provider : new LockProvider(source, provider.LockMode, provider.LockBehavior);
    }

    /// <inheritdoc/>
    protected override CompilableProvider VisitInclude(IncludeProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      _ = OnRecursionExit(provider);
      return source == provider.Source
        ? provider
        : new IncludeProvider(source, provider.Algorithm, provider.IsInlined,
            provider.FilterDataSource, provider.ResultColumnName, provider.FilteredColumns);
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

    private static Expression DefaultExpressionTranslator(CompilableProvider p, Expression e) => e;

    // Constructors

    /// <inheritdoc/>
    public CompilableProviderVisitor()
      : this(DefaultExpressionTranslator)
    {
    }

    /// <inheritdoc/>
    /// <param name="expressionTranslator">Expression translator.</param>
    public CompilableProviderVisitor(Func<CompilableProvider, Expression, Expression> expressionTranslator)
    {
      translate = expressionTranslator;
    }
  }
}