// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.24

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Rse.Resources;

namespace Xtensive.Storage.Rse.Optimization
{
  internal sealed class OrderingCorrectionRewriter : CompilableProviderVisitor
  {
    private CompilableProvider origin;
    private DirectionCollection<int> sortOrder;
    private ProviderOrderingDescriptor consumerDescriptor;
    private readonly Func<CompilableProvider, ProviderOrderingDescriptor> descriptorResolver;
    private bool orderIsCorrupted;
    private bool insideSetOperation;

    public CompilableProvider Rewrite(CompilableProvider originProvider)
    {
      ArgumentValidator.EnsureArgumentNotNull(originProvider, "originProvider");
      origin = originProvider;
      if (origin.Type==ProviderType.Select) {
        var selectProvider = (SelectProvider) origin;
        var sourceProvider = VisitCompilable(selectProvider.Source);
        if (sourceProvider==selectProvider.Source)
          return selectProvider;
        if (sortOrder!=null && sortOrder.Count > 0)
          sourceProvider = new SortProvider(sourceProvider, sortOrder);
        var provider = new SelectProvider(sourceProvider, selectProvider.ColumnIndexes);
        return provider;
      }
      else {
        var provider = VisitCompilable(origin);
        if (sortOrder!=null && sortOrder.Count > 0)
          provider = new SortProvider(provider, sortOrder);
        return provider;
      }
    }

    protected override Provider Visit(CompilableProvider cp)
    {
      var prevConsumerDescriptor = consumerDescriptor;
      consumerDescriptor = descriptorResolver(cp);

      var visited = (CompilableProvider)base.Visit(cp);
      if (consumerDescriptor.ResetsOrdering) {
        orderIsCorrupted = false;
        sortOrder = null;
      }
      visited = CorrectOrderIfNecessary(prevConsumerDescriptor, visited);
      SaveSortOrder(visited);

      consumerDescriptor = prevConsumerDescriptor;
      return RemoveSortProviderIfPossible(prevConsumerDescriptor, visited);
    }

    private static Provider RemoveSortProviderIfPossible(ProviderOrderingDescriptor prevConsumerDescriptor,
      CompilableProvider visited)
    {
      if (!prevConsumerDescriptor.IsOrderSensitive) {
        var sortProvider = visited as SortProvider;
        return sortProvider==null ? visited : sortProvider.Source;
      }
      return visited;
    }

    private void SaveSortOrder(Provider cp)
    {
      if (consumerDescriptor.IsOrdering)
        sortOrder = cp.Header.Order;
    }

    private CompilableProvider CorrectOrderIfNecessary(ProviderOrderingDescriptor prevConsumerDescriptor,
      CompilableProvider visited)
    {
      orderIsCorrupted = orderIsCorrupted || !consumerDescriptor.PreservesOrder;
      if (prevConsumerDescriptor.IsOrderSensitive && orderIsCorrupted && sortOrder != null) {
        visited = new SortProvider(visited, sortOrder);
        orderIsCorrupted = false;
      }
      return visited;
    }

    protected override Provider VisitJoin(JoinProvider provider)
    {
      var joinSortOrder = new DirectionCollection<int>();

      CompilableProvider left;
      joinSortOrder = VisitLeftJoinOperand(provider, joinSortOrder, out left);

      CompilableProvider right;
      joinSortOrder = VisitRightJoinOperant(provider, joinSortOrder, left, out right);
      if (left==provider.Left && right==provider.Right)
        return provider;
      sortOrder = joinSortOrder;
      return new JoinProvider(left, right, provider.Outer, provider.JoinType, provider.EqualIndexes);
    }

    protected override Provider VisitPredicateJoin(PredicateJoinProvider provider)
    {
      var joinSortOrder = new DirectionCollection<int>();

      CompilableProvider left;
      joinSortOrder = VisitLeftJoinOperand(provider, joinSortOrder, out left);

      CompilableProvider right;
      joinSortOrder = VisitRightJoinOperant(provider, joinSortOrder, left, out right);
      if (left==provider.Left && right==provider.Right)
        return provider;
      sortOrder = joinSortOrder;
      return new PredicateJoinProvider(left, right, provider.Predicate, provider.Outer);
    }

    private DirectionCollection<int> VisitRightJoinOperant(BinaryProvider provider,
      DirectionCollection<int> joinSortOrder, CompilableProvider left, out CompilableProvider right)
    {
      var prevSortOrder = sortOrder;
      right = VisitCompilable(provider.Right);
      if (sortOrder!=null) {
        joinSortOrder = new DirectionCollection<int>(
          joinSortOrder.Union(sortOrder.Select(p =>
            new KeyValuePair<int, Direction>(p.Key + left.Header.Length, p.Value))));
      }
      sortOrder = prevSortOrder;
      return joinSortOrder;
    }

    private DirectionCollection<int> VisitLeftJoinOperand(BinaryProvider provider,
      DirectionCollection<int> joinSortOrder, out CompilableProvider left)
    {
      var prevSortOrder = sortOrder;
      left = VisitCompilable(provider.Left);
      if (sortOrder!=null)
        joinSortOrder = sortOrder;
      sortOrder = prevSortOrder;
      return joinSortOrder;
    }

    protected override Provider VisitApply(ApplyProvider provider)
    {
      var applySortOrder = new DirectionCollection<int>();
      CompilableProvider left;
      CompilableProvider right;

      var prevSortOrder = sortOrder;
      left = VisitCompilable(provider.Left);
      if (sortOrder!=null)
        applySortOrder = sortOrder;
      sortOrder = prevSortOrder;

      prevSortOrder = sortOrder;
      right = VisitCompilable(provider.Right);
      if ((provider.ApplyType==ApplyType.Cross || provider.ApplyType==ApplyType.Outer)
        && sortOrder!=null) {
        applySortOrder = new DirectionCollection<int>(
          applySortOrder.Union(sortOrder.Select(p =>
            new KeyValuePair<int, Direction>(p.Key + left.Header.Length, p.Value))));
      }
      sortOrder = prevSortOrder;

      if (left==provider.Left && right==provider.Right)
        return provider;
      sortOrder = applySortOrder;
      return new ApplyProvider(provider.ApplyParameter, left, right, provider.ApplyType);
    }

    protected override Provider VisitAggregate(AggregateProvider provider)
    {
      var agrSortOrder = new DirectionCollection<int>();
      var prevSortOrder = sortOrder;
      Provider result = base.VisitAggregate(provider);
      if (sortOrder!=null)
        agrSortOrder = new DirectionCollection<int>(
          sortOrder.Where(p => provider.GroupColumnIndexes.Contains(p.Key)));
      sortOrder = prevSortOrder;

      if (agrSortOrder.Count > 0)
        sortOrder = agrSortOrder;
      return result;
    }

    protected override Provider VisitSelect(SelectProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      if (sortOrder != null && !insideSetOperation) {
        var outerDirectionCollection = new DirectionCollection<int>();
        foreach (KeyValuePair<int, Direction> pair in sortOrder) {
          var columnIndex = provider.ColumnIndexes.IndexOf(pair.Key);
          if (columnIndex < 0)
            throw new InvalidOperationException(
              Strings.ExItIsNotAllowedToUseSelectProviderWhichRemovesColumnsUsedForOrdering);
          outerDirectionCollection.Add(columnIndex, pair.Value);
        }
        sortOrder = outerDirectionCollection;
        return new SelectProvider(source, provider.ColumnIndexes);
      }
      return provider;
    }

    protected override Provider VisitUnion(UnionProvider provider)
    {
      return VisitSetOperation(provider, base.VisitUnion);
    }

    protected override Provider VisitConcat(ConcatProvider provider)
    {
      return VisitSetOperation(provider, base.VisitConcat);
    }

    protected override Provider VisitExcept(ExceptProvider provider)
    {
      return VisitSetOperation(provider, base.VisitExcept);
    }

    protected override Provider VisitIntersect(IntersectProvider provider)
    {
      return VisitSetOperation(provider, base.VisitIntersect);
    }

    protected override Provider VisitExistence(ExistenceProvider provider)
    {
      return VisitSetOperation(provider, base.VisitExistence);
    }

    private Provider VisitSetOperation<T>(T provider, Func<T, Provider> visitMethod)
      where T : CompilableProvider
    {
      var prevInsideSetOperation = insideSetOperation;
      insideSetOperation = true;
      var result = visitMethod(provider);
      insideSetOperation = prevInsideSetOperation;
      return result;
    }


    // Constructors

    public OrderingCorrectionRewriter(
      Func<CompilableProvider, ProviderOrderingDescriptor> orderingDescriptorResolver)
    {
      ArgumentValidator.EnsureArgumentNotNull(orderingDescriptorResolver, "orderingDescriptorResolver");
      descriptorResolver = orderingDescriptorResolver;
    }
  }
}