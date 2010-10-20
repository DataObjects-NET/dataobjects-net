// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.24

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Rse.Resources;
using System.Linq;

namespace Xtensive.Storage.Rse.PreCompilation.Correction
{
  internal class OrderingCorrectorRewriter : BaseOrderingCorrectorRewriter
  {
    private CompilableProvider origin;
    private ProviderOrderingDescriptor? descriptor;
    private ProviderOrderingDescriptor? consumerDescriptor;
    private readonly Func<CompilableProvider, ProviderOrderingDescriptor> descriptorResolver;
    private bool isOrderCorrupted;
    private bool isOrderOfIndex;

    protected DirectionCollection<int> SortOrder { get; set; }
    protected DirectionCollection<int> OriginalExpectedOrder { get; set; }

    public CompilableProvider Rewrite(CompilableProvider originProvider)
    {
      ArgumentValidator.EnsureArgumentNotNull(originProvider, "originProvider");
      origin = originProvider;
      Initialize();

      if (origin.Type==ProviderType.Select) {
        var selectProvider = (SelectProvider) origin;
        var visitedSource = VisitCompilable(selectProvider.Source);
        if (isOrderCorrupted && !isOrderOfIndex)
          visitedSource = OnInsertSortProvider(visitedSource);
        else if (isOrderOfIndex)
          visitedSource.SetActualOrdering(new DirectionCollection<int>());
        return RecreateSelectProvider(selectProvider, visitedSource);
      }
      var visited = VisitCompilable(origin);
      if (isOrderOfIndex) {
        if (!isOrderCorrupted)
          visited.SetActualOrdering(new DirectionCollection<int>());
        return visited;
      }
      return isOrderCorrupted ? OnInsertSortProvider(visited) : visited;
    }

    protected override sealed Provider Visit(CompilableProvider cp)
    {
      var prevConsumerDescriptor = consumerDescriptor;
      consumerDescriptor = descriptor;
      descriptor = descriptorResolver(cp);

      var prevDescriptor = descriptor;
      var visited = (CompilableProvider) base.Visit(cp);
      descriptor = prevDescriptor;
      OriginalExpectedOrder = cp.ExpectedOrder;

      var result = RemoveSortProvider(visited);
      if (result==visited)
        result = CorrectOrder(visited);

      descriptor = consumerDescriptor;
      consumerDescriptor = prevConsumerDescriptor;
      return result;
    }

    protected override sealed Provider VisitSelect(SelectProvider provider)
    {
      var result = provider;
      var source = VisitCompilable(provider.Source);
      if (source!=provider.Source)
        result = OnRecreateSelectProvider(provider, source);
      if (SortOrder.Count > 0 && provider.ExpectedOrder.Count==0 && consumerDescriptor!=null && !consumerDescriptor.Value.BreaksOrder && !consumerDescriptor.Value.PreservesOrder)
        OnValidateRemovingOfOrderedColumns();
      CheckCorruptionOfOrder();
      OriginalExpectedOrder = provider.ExpectedOrder;
      SaveSortOrder(result);
      return result;
    }

    protected override Provider VisitAggregate(AggregateProvider provider)
    {
      var result = provider;
      var source = VisitCompilable(provider.Source);
      if (source != provider.Source)
        result = OnRecreateAggregateProvider(provider, source);
      CheckCorruptionOfOrder();
      OriginalExpectedOrder = provider.ExpectedOrder;
      SaveSortOrder(result);
      return result;
    }

    protected override sealed Provider VisitIndex(IndexProvider provider)
    {
      SortOrder = provider.ExpectedOrder;
      // If current IndexProvider does not preserve records order, 
      // then we reset value of Header.Order
      if (!descriptor.Value.PreservesOrder)
        SetActualOrdering(provider, new DirectionCollection<int>());
      return provider;
    }

    protected override Provider VisitApply(ApplyProvider provider)
    {
      OnRecursionEntrance(provider);
      var left = provider.Left;
      var right = provider.Right;
      VisitBinaryProviderSources(ref left, ref right);
      OnRecursionExit(provider);
      if (left == provider.Left && right == provider.Right)
        return provider;
      return new ApplyProvider(provider.ApplyParameter, left, right, provider.IsInlined, provider.SequenceType, provider.ApplyType);
    }

    protected override Provider VisitIntersect(IntersectProvider provider)
    {
      OnRecursionEntrance(provider);
      var left = provider.Left;
      var right = provider.Right;
      VisitBinaryProviderSources(ref left, ref right);
      OnRecursionExit(provider);
      if (left == provider.Left && right == provider.Right)
        return provider;
      return new IntersectProvider(left, right);
    }

    protected override Provider VisitExcept(ExceptProvider provider)
    {
      OnRecursionEntrance(provider);
      var left = provider.Left;
      var right = provider.Right;
      VisitBinaryProviderSources(ref left, ref right);
      OnRecursionExit(provider);
      if (left == provider.Left && right == provider.Right)
        return provider;
      return new ExceptProvider(left, right);
    }

    protected override Provider VisitConcat(ConcatProvider provider)
    {
      OnRecursionEntrance(provider);
      var left = provider.Left;
      var right = provider.Right;
      VisitBinaryProviderSources(ref left, ref right);
      OnRecursionExit(provider);
      if (left == provider.Left && right == provider.Right)
        return provider;
      return new ConcatProvider(left, right);
    }

    protected override Provider VisitUnion(UnionProvider provider)
    {
      OnRecursionEntrance(provider);
      var left = provider.Left;
      var right = provider.Right;
      VisitBinaryProviderSources(ref left, ref right);
      OnRecursionExit(provider);
      if (left == provider.Left && right == provider.Right)
        return provider;
      return new UnionProvider(left, right);
    }

    protected override Provider VisitJoin(JoinProvider provider)
    {
      OnRecursionEntrance(provider);
      var left = provider.Left;
      var right = provider.Right;
      VisitBinaryProviderSources(ref left, ref right);
      var equalIndexes = OnRecursionExit(provider);
      if (left == provider.Left && right == provider.Right)
        return provider;
      return new JoinProvider(left, right, provider.JoinType, provider.JoinAlgorithm,
        equalIndexes != null ? (Pair<int>[])equalIndexes : provider.EqualIndexes);
    }

    protected override Provider VisitPredicateJoin(PredicateJoinProvider provider)
    {
      OnRecursionEntrance(provider);
      var left = provider.Left;
      var right = provider.Right;
      VisitBinaryProviderSources(ref left, ref right);
      var predicate = (Expression<Func<Tuple, Tuple, bool>>)OnRecursionExit(provider);
      if (left == provider.Left && right == provider.Right)
        return provider;
      return new PredicateJoinProvider(left, right, predicate ?? provider.Predicate, provider.JoinType);
    }

    protected virtual void OnValidateRemovingOfOrderedColumns()
    {
      if (!isOrderOfIndex)
        throw new InvalidOperationException(Strings.ExSelectProviderRemovesColumnsUsedForOrdering);
    }

    protected SelectProvider OnRecreateSelectProvider(SelectProvider modifiedProvider, CompilableProvider originalSource)
    {
      return new SelectProvider(originalSource, modifiedProvider.ColumnIndexes);
    }

    protected AggregateProvider OnRecreateAggregateProvider(AggregateProvider modifiedProvider, CompilableProvider originalSource)
    {
      var acd = new List<AggregateColumnDescriptor>(modifiedProvider.AggregateColumns.Length);
      acd.AddRange(modifiedProvider.AggregateColumns.Select(ac => new AggregateColumnDescriptor(ac.Name, ac.SourceIndex, ac.AggregateType)));
      return new AggregateProvider(originalSource, modifiedProvider.GroupColumnIndexes, acd.ToArray());
    }

    protected virtual Provider OnRemoveSortProvider(SortProvider sortProvider)
    {
      SortOrder = sortProvider.Order;
      isOrderOfIndex = false;
      isOrderCorrupted = true;
      return sortProvider.Source;
    }

    protected virtual CompilableProvider OnInsertSortProvider(CompilableProvider visited)
    {
      isOrderCorrupted = false;
      if (SortOrder==null || SortOrder.Count==0)
        return visited;
      return new SortProvider(visited, SortOrder);
    }

    #region Private \ internal methods

    private void Initialize()
    {
      descriptor = null;
      consumerDescriptor = null;
      isOrderCorrupted = false;
      SortOrder = new DirectionCollection<int>();
    }

    private Provider RemoveSortProvider(CompilableProvider visited)
    {
      if (consumerDescriptor!=null && !consumerDescriptor.Value.IsOrderSensitive) {
        var sortProvider = visited as SortProvider;
        if (sortProvider!=null)
          return OnRemoveSortProvider(sortProvider);
      }
      return visited;
    }

    private static CompilableProvider RecreateSelectProvider(SelectProvider selectProvider,
      CompilableProvider visitedSource)
    {
      if (visitedSource==selectProvider.Source)
        return selectProvider;
      return new SelectProvider(visitedSource, selectProvider.ColumnIndexes);
    }

    private CompilableProvider CorrectOrder(CompilableProvider visited)
    {
      var result = visited;
      CheckCorruptionOfOrder();

      if (isOrderCorrupted && !descriptor.Value.IsSorter
        && consumerDescriptor!=null && consumerDescriptor.Value.IsOrderSensitive)
        result = OnInsertSortProvider(visited);

      if (!(result is SelectProvider)) {
        SaveSortOrder(result);
      }
      return result;
    }

    private void SaveSortOrder(CompilableProvider result)
    {
      if (!isOrderCorrupted)
        SetActualOrdering(result, result.ExpectedOrder);

      if (descriptor.Value.BreaksOrder) {
        isOrderCorrupted = true;
        SortOrder = new DirectionCollection<int>();
      }
      else
        SortOrder = isOrderCorrupted ? OriginalExpectedOrder : new DirectionCollection<int>();

      if (descriptor.Value.IsSorter)
        isOrderOfIndex = false;
      else if (result is IndexProvider)
        isOrderOfIndex = true;
    }

    private void CheckCorruptionOfOrder()
    {
      isOrderCorrupted = !descriptor.Value.IsSorter
        && (isOrderCorrupted || !descriptor.Value.PreservesOrder);
    }

    private void VisitBinaryProviderSources(ref CompilableProvider left, ref CompilableProvider right)
    {
      left = VisitCompilable(left);
      var isOrderOfIndexForLeft = isOrderOfIndex;
      isOrderOfIndex = false;
      right = VisitCompilable(right);
      isOrderOfIndex = isOrderOfIndexForLeft && isOrderOfIndex;
    }

    #endregion

    // Constructors

    public OrderingCorrectorRewriter(Func<CompilableProvider, ProviderOrderingDescriptor> orderingDescriptorResolver)
    {
      ArgumentValidator.EnsureArgumentNotNull(orderingDescriptorResolver, "orderingDescriptorResolver");
      descriptorResolver = orderingDescriptorResolver;
    }
  }
}