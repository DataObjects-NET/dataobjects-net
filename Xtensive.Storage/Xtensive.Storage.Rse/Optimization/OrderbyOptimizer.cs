// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.30

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Optimization.Implementation;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.Optimization
{
  /// <summary>
  /// Order by <see cref="IOptimizer"/> implementation.
  /// </summary>
  [Serializable]
  public sealed class OrderbyOptimizer : IOptimizer
  {
    private readonly OrderingCorrectionRewriter rewriter;

    /// <inheritdoc/>
    CompilableProvider IOptimizer.Optimize(CompilableProvider rootProvider)
    {
      return rewriter.Rewrite(rootProvider);
    }


    // Constructors

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="orderingDescriptorResolver">The resolver of 
    /// <see cref="ProviderOrderingDescriptor"/>.</param>
    public OrderbyOptimizer(
      Func<CompilableProvider, ProviderOrderingDescriptor> orderingDescriptorResolver)
    {
      ArgumentValidator.EnsureArgumentNotNull(orderingDescriptorResolver, "orderingDescriptorResolver");
      rewriter = new OrderingCorrectionRewriter(orderingDescriptorResolver);
    }
  }
}