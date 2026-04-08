// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.30

using System;
using Xtensive.Core;

using Xtensive.Orm.Rse.Compilation;
using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm.Rse.Transformation
{
  /// <summary>
  /// Corrects an ordering of records.
  /// </summary>
  [Serializable]
  public sealed class OrderingCorrector : IPreCompiler
  {
    public static OrderingCorrector DefaultInstance { get; } = new OrderingCorrector();


    private readonly Func<CompilableProvider, ProviderOrderingDescriptor> orderingDescriptorResolver;

    /// <inheritdoc/>
    CompilableProvider IPreCompiler.Process(CompilableProvider rootProvider)
    {
      return OrderingRewriter.Rewrite(rootProvider, orderingDescriptorResolver);
    }

    private static ProviderOrderingDescriptor ResolveOrderingDescriptor(CompilableProvider provider)
    {
      var isOrderSensitive = provider.Type is ProviderType.Skip
        or ProviderType.Take
        or ProviderType.Seek
        or ProviderType.Paging
        or ProviderType.RowNumber;
      var preservesOrder = provider.Type is ProviderType.Skip
        or ProviderType.Take
        or ProviderType.Seek
        or ProviderType.Paging
        or ProviderType.RowNumber
        or ProviderType.Distinct
        or ProviderType.Alias;
      var isOrderBreaker = provider.Type is ProviderType.Except
        or ProviderType.Intersect
        or ProviderType.Union
        or ProviderType.Concat
        or ProviderType.Existence;
      var isSorter = provider.Type is ProviderType.Sort or ProviderType.Index;
      return new ProviderOrderingDescriptor(isOrderSensitive, preservesOrder, isOrderBreaker, isSorter);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="orderingDescriptorResolver">The resolver of
    /// <see cref="ProviderOrderingDescriptor"/>.</param>
    public OrderingCorrector(Func<CompilableProvider, ProviderOrderingDescriptor> orderingDescriptorResolver)
    {
      this.orderingDescriptorResolver = orderingDescriptorResolver ?? throw new ArgumentNullException(nameof(orderingDescriptorResolver));
    }

    private OrderingCorrector()
    {
      this.orderingDescriptorResolver = ResolveOrderingDescriptor;
    }
  }
}