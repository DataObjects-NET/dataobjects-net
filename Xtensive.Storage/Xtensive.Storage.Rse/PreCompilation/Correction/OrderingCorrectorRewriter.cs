// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.24

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Storage.Rse.PreCompilation.Optimization;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Rse.Resources;

namespace Xtensive.Storage.Rse.PreCompilation.Correction
{
  internal class OrderingCorrectorRewriter : BaseOrderingCorrectorRewriter
  {
    private CompilableProvider origin;
    private ProviderOrderingDescriptor? descriptor;
    private ProviderOrderingDescriptor? consumerDescriptor;
    private readonly Func<CompilableProvider, ProviderOrderingDescriptor> descriptorResolver;
    private bool orderIsCorrupted;

    protected DirectionCollection<int> SortOrder { get; set; }

    public CompilableProvider Rewrite(CompilableProvider originProvider)
    {
      ArgumentValidator.EnsureArgumentNotNull(originProvider, "originProvider");
      origin = originProvider;
      descriptor = null;
      SortOrder = new DirectionCollection<int>();

      if (origin.Type==ProviderType.Select) {
        var selectProvider = (SelectProvider) origin;
        var visitedSource = VisitCompilable(selectProvider.Source);
        visitedSource = OnInsertSortProvider(visitedSource);
        return RecreateSelectProvider(selectProvider, visitedSource);
      }
      var provider = VisitCompilable(origin);
      return OnInsertSortProvider(provider);
    }

    protected sealed override Provider Visit(CompilableProvider cp)
    {
      var prevConsumerDescriptor = consumerDescriptor;
      consumerDescriptor = descriptor;
      descriptor = descriptorResolver(cp);

      var prevDescriptor = descriptor;
      var visited = (CompilableProvider) base.Visit(cp);
      descriptor = prevDescriptor;

      var result = RemoveSortProvider(visited);
      if (result==visited)
        result = CorrectOrder(visited);

      descriptor = consumerDescriptor;
      consumerDescriptor = prevConsumerDescriptor;
      return result;
    }

    protected sealed override Provider VisitSelect(SelectProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      if(source != provider.Source)
        provider = OnRecreateSelectProvider(provider, source);
      if(SortOrder.Count > 0 && provider.ExpectedOrder.Count == 0
        && !consumerDescriptor.Value.BreaksOrder)
        OnValidateRemovingOfOrderedColumns();
      CheckCorruptionOfOrder();
      if (!orderIsCorrupted)
      {
        SetActualOrdering(provider, provider.ExpectedOrder);
        SortOrder = provider.Header.Order;
      }
      else
        SortOrder = new DirectionCollection<int>();
      return provider;
    }

    protected virtual void OnValidateRemovingOfOrderedColumns()
    {
      throw new InvalidOperationException(
            Strings.ExSelectProviderRemovesColumnsUsedForOrdering);
    }

    protected SelectProvider OnRecreateSelectProvider(SelectProvider modifiedProvider,
      CompilableProvider originalSource)
    {
      return new SelectProvider(originalSource, modifiedProvider.ColumnIndexes);
    }

    protected virtual Provider OnRemoveSortProvider(SortProvider sortProvider)
    {
      SortOrder = sortProvider.Order;
      orderIsCorrupted = true;
      return sortProvider.Source;
    }

    protected virtual CompilableProvider OnInsertSortProvider(CompilableProvider visited)
    {
      orderIsCorrupted = false;
      return SortOrder == null || SortOrder.Count == 0 ? visited : new SortProvider(visited, SortOrder);
    }

    #region Private \ internal methods

    private Provider RemoveSortProvider(CompilableProvider visited)
    {
      if (consumerDescriptor != null 
        && !consumerDescriptor.Value.IsOrderSensitive) {
        var sortProvider = visited as SortProvider;
        if (sortProvider != null)
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
      if (orderIsCorrupted && consumerDescriptor != null
        && consumerDescriptor.Value.IsOrderSensitive)
        result = OnInsertSortProvider(visited);
      if (!(result is SelectProvider)) {
        if (!orderIsCorrupted)
          SetActualOrdering(result, result.ExpectedOrder);
        if (!descriptor.Value.BreaksOrder)
          SortOrder = orderIsCorrupted ? result.ExpectedOrder : new DirectionCollection<int>();
        else {
          orderIsCorrupted = true;
          SortOrder = new DirectionCollection<int>();
        }
      }
      return result;
    }

    private void CheckCorruptionOfOrder()
    {
      orderIsCorrupted = orderIsCorrupted || !descriptor.Value.PreservesOrder;
    }

    #endregion


    // Constructors

    public OrderingCorrectorRewriter(
      Func<CompilableProvider, ProviderOrderingDescriptor> orderingDescriptorResolver)
    {
      ArgumentValidator.EnsureArgumentNotNull(orderingDescriptorResolver, "orderingDescriptorResolver");
      descriptorResolver = orderingDescriptorResolver;
    }
  }
}