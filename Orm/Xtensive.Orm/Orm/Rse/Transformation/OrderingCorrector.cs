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
    private readonly Func<CompilableProvider, ProviderOrderingDescriptor> orderingDescriptorResolver;

    /// <inheritdoc/>
    CompilableProvider IPreCompiler.Process(CompilableProvider rootProvider)
    {
      return OrderingRewriter.Rewrite(rootProvider, orderingDescriptorResolver);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="orderingDescriptorResolver">The resolver of
    /// <see cref="ProviderOrderingDescriptor"/>.</param>
    public OrderingCorrector(Func<CompilableProvider, ProviderOrderingDescriptor> orderingDescriptorResolver)
    {
      ArgumentValidator.EnsureArgumentNotNull(orderingDescriptorResolver, "orderingDescriptorResolver");
      this.orderingDescriptorResolver = orderingDescriptorResolver;
    }
  }
}