// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.10

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Rse.Providers.Compilable;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using System.Linq;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// <see cref="CompilableProvider"/> visitor class. Result is <see cref="CompilableProvider"/>.
  /// </summary>
  [Serializable]
  public class CompilableProviderVisitor : ProviderVisitor
  {
    protected Func<Provider, Expression, Expression> translate;

    /// <summary>
    /// Visits the compilable provider.
    /// </summary>
    /// <param name="cp">The compilable provider.</param>
    public CompilableProvider VisitCompilable(CompilableProvider cp)
    {
      var result = (CompilableProvider)Visit(cp);
      return result;
    }

    
    protected override Provider VisitTake(TakeProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      OnRecursionExit(provider);
      if (source == provider.Source)
        return provider;
      return new TakeProvider(source, provider.Count);
    }

    
    protected override Provider VisitSkip(SkipProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      OnRecursionExit(provider);
      if (source == provider.Source)
        return provider;
      return new SkipProvider(source, provider.Count);
    }

    
    protected override Provider VisitPaging(PagingProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      OnRecursionExit(provider);
      if (source == provider.Source)
        return provider;
      return new PagingProvider(source, provider);
    }

    
    protected override Provider VisitSelect(SelectProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      var columnIndexes = (int[])OnRecursionExit(provider);
      if (source == provider.Source)
        return provider;
      return new SelectProvider(source, columnIndexes ?? provider.ColumnIndexes);
    }

    
    protected override Provider VisitSeek(SeekProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      OnRecursionExit(provider);
      if (source==provider.Source)
        return provider;
      return new SeekProvider(source, provider.Key);
    }

    
    protected override Provider VisitRaw(RawProvider provider)
    {
      return provider;
    }

    
    protected override Provider VisitSort(SortProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      var order = OnRecursionExit(provider);
      if (source == provider.Source)
        return provider;
      return new SortProvider(source, (order == null) ? provider.Order : (DirectionCollection<int>)order);
    }

    
    protected override Provider VisitJoin(JoinProvider provider)
    {
      OnRecursionEntrance(provider);
      var left = VisitCompilable(provider.Left);
      var right = VisitCompilable(provider.Right);
      var equalIndexes = OnRecursionExit(provider);
      if (left == provider.Left && right == provider.Right)
        return provider;
      return new JoinProvider(left, right, provider.JoinType,
        equalIndexes != null ? (Pair<int>[])equalIndexes : provider.EqualIndexes);
    }

    
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

    
    protected override Provider VisitDistinct(DistinctProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      OnRecursionExit(provider);
      if (source == provider.Source)
        return provider;
      return new DistinctProvider(source);
    }

    
    protected override Provider VisitCalculate(CalculateProvider provider)
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
      return new CalculateProvider(source, descriptors.ToArray());
    }

    protected override Provider VisitRowNumber(RowNumberProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      OnRecursionExit(provider);
      if (source == provider.Source)
        return provider;
      return new RowNumberProvider(source, provider.SystemColumn.Name);
    }


    
    protected override Provider VisitAlias(AliasProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      OnRecursionExit(provider);
      if (source == provider.Source)
        return provider;
      return new AliasProvider(source, provider.Alias);
    }

    
    protected override Provider VisitAggregate(AggregateProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      var resultParameters = OnRecursionExit(provider);
      if (source == provider.Source)
        return provider;
      if (resultParameters == null) {
        var acd = new List<AggregateColumnDescriptor>(provider.AggregateColumns.Length);
        acd.AddRange(provider.AggregateColumns.Select(ac => new AggregateColumnDescriptor(ac.Name, ac.SourceIndex, ac.AggregateType)));
        return new AggregateProvider(source, provider.GroupColumnIndexes, acd.ToArray());
      }
      var result = (Pair<int[], AggregateColumnDescriptor[]>) resultParameters;
      return new AggregateProvider(source, result.First, result.Second);
    }

    
    protected override Provider VisitStore(StoreProvider provider)
    {
      var compilableSource = provider.Source as CompilableProvider;
      if (compilableSource == null)
        return provider;
      OnRecursionEntrance(provider);
      var source = VisitCompilable(compilableSource);
      OnRecursionExit(provider);
      if (source == compilableSource)
        return provider;
      return new StoreProvider(source, provider.Scope, provider.Name);
    }

    
    protected override Provider VisitIndex(IndexProvider provider)
    {
      OnRecursionEntrance(provider);
      OnRecursionExit(provider);
      return provider;
    }

    
    protected override Provider VisitFreeText(FreeTextProvider provider)
    {
      OnRecursionEntrance(provider);
      OnRecursionExit(provider);
      return provider;
    }

    
    protected override Provider VisitPredicateJoin(PredicateJoinProvider provider)
    {
      OnRecursionEntrance(provider);
      var left = VisitCompilable(provider.Left);
      var right = VisitCompilable(provider.Right);
      var predicate = (Expression<Func<Tuple, Tuple, bool>>)OnRecursionExit(provider);
      if (left == provider.Left && right == provider.Right)
        return provider;
      return new PredicateJoinProvider(left, right, predicate ?? provider.Predicate, provider.JoinType);
    }

    
    protected override Provider VisitExistence(ExistenceProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      OnRecursionExit(provider);
      if (source == provider.Source)
        return provider;
      return new ExistenceProvider(source, provider.ExistenceColumnName);
    }

    
    protected override Provider VisitApply(ApplyProvider provider)
    {
      OnRecursionEntrance(provider);
      var left = VisitCompilable(provider.Left);
      var right = VisitCompilable(provider.Right);
      OnRecursionExit(provider);
      if (left == provider.Left && right == provider.Right)
        return provider;
      return new ApplyProvider(provider.ApplyParameter, left, right, provider.IsInlined, provider.SequenceType, provider.ApplyType);
    }

    
    protected override Provider VisitIntersect(IntersectProvider provider)
    {
      OnRecursionEntrance(provider);
      var left = VisitCompilable(provider.Left);
      var right = VisitCompilable(provider.Right);
      OnRecursionExit(provider);
      if (left == provider.Left && right == provider.Right)
        return provider;
      return new IntersectProvider(left, right);
    }

    
    protected override Provider VisitExcept(ExceptProvider provider)
    {
      OnRecursionEntrance(provider);
      var left = VisitCompilable(provider.Left);
      var right = VisitCompilable(provider.Right);
      OnRecursionExit(provider);
      if (left == provider.Left && right == provider.Right)
        return provider;
      return new ExceptProvider(left, right);
    }

    
    protected override Provider VisitConcat(ConcatProvider provider)
    {
      OnRecursionEntrance(provider);
      var left = VisitCompilable(provider.Left);
      var right = VisitCompilable(provider.Right);
      OnRecursionExit(provider);
      if (left == provider.Left && right == provider.Right)
        return provider;
      return new ConcatProvider(left, right);
    }

    
    protected override Provider VisitUnion(UnionProvider provider)
    {
      OnRecursionEntrance(provider);
      var left = VisitCompilable(provider.Left);
      var right = VisitCompilable(provider.Right);
      OnRecursionExit(provider);
      if (left == provider.Left && right == provider.Right)
        return provider;
      return new UnionProvider(left, right);
    }

    
    protected override Provider VisitLock(LockProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      OnRecursionExit(provider);
      if (source == provider.Source)
        return provider;
      return new LockProvider(source, provider.LockMode, provider.LockBehavior);
    }

    protected override Provider VisitInclude(IncludeProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      OnRecursionExit(provider);
      if (source == provider.Source)
        return provider;
      return new IncludeProvider(source, provider.Algorithm, provider.IsInlined,
        provider.FilterDataSource, provider.ResultColumnName, provider.FilteredColumns);
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

    
    public CompilableProviderVisitor()
      : this(DefaultExpressionTranslator)
    {
    }

    
    /// <param name="expressionTranslator">Expression translator.</param>
    public CompilableProviderVisitor(Func<Provider, Expression, Expression> expressionTranslator)
    {
      translate = expressionTranslator;
    }
  }
}