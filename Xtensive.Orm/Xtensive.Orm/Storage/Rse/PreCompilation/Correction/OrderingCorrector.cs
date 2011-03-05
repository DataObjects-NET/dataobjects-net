// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.30

using System;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.PreCompilation.Correction
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
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
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