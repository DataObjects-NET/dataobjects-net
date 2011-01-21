// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.24

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Tuple = Xtensive.Core.Tuples.Tuple;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Rse.Resources;
using System.Linq;

namespace Xtensive.Storage.Rse.PreCompilation.Correction
{
  internal sealed class OrderingRewriter : CompilableProviderVisitor
  {
    private readonly Func<CompilableProvider, ProviderOrderingDescriptor> descriptorResolver;
    private DirectionCollection<int> sortOrder;
    private ProviderOrderingDescriptor descriptor;
    private ProviderOrderingDescriptor consumerDescriptor;

    public static CompilableProvider Rewrite(
      CompilableProvider originProvider, 
      Func<CompilableProvider, ProviderOrderingDescriptor> orderingDescriptorResolver)
    {
      ArgumentValidator.EnsureArgumentNotNull(originProvider, "originProvider");
      var rewriter = new OrderingRewriter(orderingDescriptorResolver);
      if (originProvider.Type == ProviderType.Select) {
        var selectProvider = (SelectProvider) originProvider;
        var source = rewriter.VisitCompilable(selectProvider.Source);
        return new SelectProvider(
          rewriter.InsertSortProvider(source), 
          selectProvider.ColumnIndexes);
      }
      var visited = rewriter.VisitCompilable(originProvider);
      return rewriter.InsertSortProvider(visited);
    }

    protected override Provider Visit(CompilableProvider cp)
    {
      var prevConsumerDescriptor = consumerDescriptor;
      consumerDescriptor = descriptor;
      descriptor = descriptorResolver(cp);

      var prevDescriptor = descriptor;
      var visited = (CompilableProvider)base.Visit(cp);
      descriptor = prevDescriptor;
      visited = CorrectOrder(visited);

      descriptor = consumerDescriptor;
      consumerDescriptor = prevConsumerDescriptor;
      return visited;
    }

    protected override Provider VisitSort(SortProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      sortOrder = provider.Order;
      if (consumerDescriptor.IsOrderSensitive)
        return source == provider.Source 
          ? provider 
          : new SortProvider(source, provider.Order);
      return source;
    }

    protected override Provider VisitSelect(SelectProvider provider)
    {
      var result = provider;
      var source = VisitCompilable(provider.Source);
      if (source != provider.Source)
        result = new SelectProvider(source, provider.ColumnIndexes);

      if (sortOrder.Count > 0) {
        var selectOrdering = new DirectionCollection<int>();
        foreach (var pair in sortOrder) {
          var columnIndex = result.ColumnIndexes.IndexOf(pair.Key);
          if (columnIndex < 0) {
            if (selectOrdering.Count > 0)
              selectOrdering.Clear();
            break;
          }
          selectOrdering.Add(columnIndex, pair.Value);
        }
        sortOrder = selectOrdering;
      }

      if (sortOrder.Count > 0 
        && provider.Header.Order.Count==0 
        && !consumerDescriptor.BreaksOrder 
        && !consumerDescriptor.PreservesOrder) {
        throw new InvalidOperationException(Strings.ExSelectProviderRemovesColumnsUsedForOrdering);
      }
      return result;
    }

    protected override Provider VisitAggregate(AggregateProvider provider)
    {
      var result = provider;
      var source = VisitCompilable(provider.Source);
      if (source != provider.Source) {
        var acds =provider.AggregateColumns
           .Select(ac => new AggregateColumnDescriptor(ac.Name, ac.SourceIndex, ac.AggregateType));
        result = new AggregateProvider(source, provider.GroupColumnIndexes, acds.ToArray());
      }
      if (sortOrder.Count > 0) {
        var selectOrdering = new DirectionCollection<int>();
        foreach (var pair in sortOrder) {
          var columnIndex = result.GroupColumnIndexes.IndexOf(pair.Key);
          if (columnIndex < 0) {
            if (selectOrdering.Count > 0)
              selectOrdering.Clear();
            break;
          }
          selectOrdering.Add(columnIndex, pair.Value);
        }
        sortOrder = selectOrdering;
      }

      return result;
    }

    protected override Provider VisitIndex(IndexProvider provider)
    {
      sortOrder = new DirectionCollection<int>();
      return provider;
    }

    protected override Provider VisitFreeText(FreeTextProvider provider)
    {
      sortOrder = new DirectionCollection<int>();
      return provider;
    }

    protected override Provider VisitRaw(RawProvider provider)
    {
      sortOrder = new DirectionCollection<int>();
      return provider;
    }

    protected override Provider VisitReindex(ReindexProvider provider)
    {
      var reindex = base.VisitReindex(provider);
      sortOrder = reindex.Header.Order;
      return reindex;
    }

    protected override Provider VisitStore(StoreProvider provider)
    {
      sortOrder = new DirectionCollection<int>();
      return provider;
    }

    protected override Provider VisitApply(ApplyProvider provider)
    {
      var left = VisitCompilable(provider.Left);
      var leftOrder = sortOrder;
      var right = VisitCompilable(provider.Right);
      var rightOrder = sortOrder;
      var result = left == provider.Left && right == provider.Right
        ? provider
        : new ApplyProvider(provider.ApplyParameter, left, right, provider.IsInlined, provider.SequenceType, provider.ApplyType);
      sortOrder = ComputeBinaryOrder(provider, leftOrder, rightOrder);
      return result;
    }

    protected override Provider VisitJoin(JoinProvider provider)
    {
      var left = VisitCompilable(provider.Left);
      var leftOrder = sortOrder;
      var right = VisitCompilable(provider.Right);
      var rightOrder = sortOrder;
      var result = left == provider.Left && right == provider.Right
        ? provider
        : new JoinProvider(left, right, provider.JoinType, provider.JoinAlgorithm, provider.EqualIndexes);
      sortOrder = ComputeBinaryOrder(provider, leftOrder, rightOrder);
      return result;
    }

    protected override Provider VisitPredicateJoin(PredicateJoinProvider provider)
    {
      var left = VisitCompilable(provider.Left);
      var leftOrder = sortOrder;
      var right = VisitCompilable(provider.Right);
      var rightOrder = sortOrder;
      var result = left == provider.Left && right == provider.Right
        ? provider
        : new PredicateJoinProvider(left, right, provider.Predicate, provider.JoinType);
      sortOrder = ComputeBinaryOrder(provider, leftOrder, rightOrder);
      return result;
    }

    #region Private \ internal methods

    private CompilableProvider InsertSortProvider(CompilableProvider visited)
    {
      return sortOrder.Count==0 
        ? visited 
        : new SortProvider(visited, sortOrder);
    }

    private CompilableProvider CorrectOrder(CompilableProvider visited)
    {
      var result = visited;
      if (sortOrder.Count > 0) {
        if (descriptor.IsSorter)
          return result;
        if (descriptor.BreaksOrder) {
          sortOrder = new DirectionCollection<int>();
          return result;
        }
        if (consumerDescriptor.IsOrderSensitive && !descriptor.IsOrderSensitive)
          result = InsertSortProvider(visited);
      }
      return result;
    }

    private static DirectionCollection<int> ComputeBinaryOrder(BinaryProvider provider, DirectionCollection<int> leftOrder, DirectionCollection<int> rightOrder)
    {
      if (leftOrder.Count > 0)
        return new DirectionCollection<int>(
          leftOrder.Concat(
            rightOrder.Select(p => new KeyValuePair<int, Direction>(p.Key + provider.Left.Header.Length, p.Value))));
      return new DirectionCollection<int>();
    }

    #endregion

    // Constructors

    private OrderingRewriter(Func<CompilableProvider, ProviderOrderingDescriptor> orderingDescriptorResolver)
    {
      ArgumentValidator.EnsureArgumentNotNull(orderingDescriptorResolver, "orderingDescriptorResolver");
      descriptorResolver = orderingDescriptorResolver;
      sortOrder = new DirectionCollection<int>();
    }
  }
}