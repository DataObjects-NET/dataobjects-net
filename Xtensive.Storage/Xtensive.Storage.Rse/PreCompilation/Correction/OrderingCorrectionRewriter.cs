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
using Xtensive.Storage.Rse.PreCompilation.Optimization;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Rse.Resources;

namespace Xtensive.Storage.Rse.PreCompilation.Correction
{
  internal sealed class OrderingCorrectionRewriter : CompilableProviderVisitor
  {
    private CompilableProvider origin;
    private DirectionCollection<int> sortOrder;
    private ProviderOrderingDescriptor? descriptor;
    private ProviderOrderingDescriptor? consumerDescriptor;
    private readonly Func<CompilableProvider, ProviderOrderingDescriptor> descriptorResolver;
    private bool orderIsCorrupted;
    private readonly bool setActualOrderOnly;

    public CompilableProvider Rewrite(CompilableProvider originProvider)
    {
      ArgumentValidator.EnsureArgumentNotNull(originProvider, "originProvider");
      origin = originProvider;
      descriptor = null;

      if (origin.Type==ProviderType.Select) {
        var selectProvider = (SelectProvider) origin;
        var visitedSource = VisitCompilable(selectProvider.Source);
        if (!setActualOrderOnly)
          visitedSource = InsertSortProvider(visitedSource);
        return RecreateSelectProvider(selectProvider, visitedSource);
      }
      var provider = VisitCompilable(origin);
      return setActualOrderOnly ? provider : InsertSortProvider(provider);
    }

    private static CompilableProvider RecreateSelectProvider(SelectProvider selectProvider,
      CompilableProvider visitedSource)
    {
      if (visitedSource==selectProvider.Source)
        return selectProvider;
      return new SelectProvider(visitedSource, selectProvider.ColumnIndexes);
    }

    private CompilableProvider InsertSortProvider(CompilableProvider sourceProvider)
    {
      var result = sourceProvider;
      if (sortOrder != null && sortOrder.Count > 0)
        result = new SortProvider(sourceProvider, sortOrder);
      return result;
    }

    protected override Provider Visit(CompilableProvider cp)
    {
      var prevConsumerDescriptor = consumerDescriptor;
      consumerDescriptor = descriptor;
      descriptor = descriptorResolver(cp);

      var prevDescriptor = descriptor;
      var visited = (CompilableProvider) base.Visit(cp);
      descriptor = prevDescriptor;

      var result = RemoveSortProviderIfPossible(visited);
      if (result==visited)
        result = CorrectOrderIfNecessary(visited);

      descriptor = consumerDescriptor;
      consumerDescriptor = prevConsumerDescriptor;
      return result;
    }

    private Provider RemoveSortProviderIfPossible(CompilableProvider visited)
    {
      if (!setActualOrderOnly && consumerDescriptor != null 
        && !consumerDescriptor.Value.IsOrderSensitive) {
        var sortProvider = visited as SortProvider;
        if (sortProvider != null) {
          sortOrder = sortProvider.Order;
          orderIsCorrupted = true;
          return sortProvider.Source;
        }
      }
      return visited;
    }

    private CompilableProvider CorrectOrderIfNecessary(CompilableProvider visited)
    {
      var result = visited;
      CheckCorruptionOfOrder();
      if (!setActualOrderOnly && orderIsCorrupted && consumerDescriptor != null 
        && consumerDescriptor.Value.IsOrderSensitive)
      {
        result = new SortProvider(visited, sortOrder);
        orderIsCorrupted = false;
      }
      if (!(result is SelectProvider)) {
        if (!orderIsCorrupted)
          result.SetActualOrdering(result.ExpectedOrder);
        sortOrder = orderIsCorrupted ? result.ExpectedOrder : result.Header.Order;
      }
      return result;
    }

    private void CheckCorruptionOfOrder()
    {
      orderIsCorrupted = orderIsCorrupted || !descriptor.Value.PreservesOrder;
    }

    protected override Provider VisitSelect(SelectProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      if(!setActualOrderOnly && source != provider.Source)
        provider = new SelectProvider(source, provider.ColumnIndexes);
      CheckCorruptionOfOrder();
      var selectOrdering = provider.ExpectedOrder;
      if(provider.ExpectedOrder.Count > 0 && !consumerDescriptor.Value.IsOrderBreaker) {
        selectOrdering = new DirectionCollection<int>();
        foreach (KeyValuePair<int, Direction> pair in provider.ExpectedOrder) {
          var columnIndex = provider.ColumnIndexes.IndexOf(pair.Key);
          if (columnIndex < 0)
            throw new InvalidOperationException(
              Strings.ExItIsNotAllowedToUseSelectProviderWhichRemovesColumnsUsedForOrdering);
          selectOrdering.Add(columnIndex, pair.Value);
        }
      }
      if (!orderIsCorrupted) {
        provider.SetActualOrdering(selectOrdering);
        sortOrder = selectOrdering;
      }
      else
        sortOrder = provider.Header.Order;
      return provider;
    }


    // Constructors

    public OrderingCorrectionRewriter(
      Func<CompilableProvider, ProviderOrderingDescriptor> orderingDescriptorResolver,
      bool setActualOrderOnly)
    {
      ArgumentValidator.EnsureArgumentNotNull(orderingDescriptorResolver, "orderingDescriptorResolver");
      descriptorResolver = orderingDescriptorResolver;
      this.setActualOrderOnly = setActualOrderOnly;
    }
  }
}