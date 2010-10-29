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
    private readonly bool setActualOrderOnly;

    /// <inheritdoc/>
    CompilableProvider IPreCompiler.Process(CompilableProvider rootProvider)
    {
      if (!setActualOrderOnly)
        return new OrderingCorrectorRewriter(orderingDescriptorResolver)
          .Rewrite(rootProvider);
      return new ActualOrderSetter(orderingDescriptorResolver).Rewrite(rootProvider);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="orderingDescriptorResolver">The resolver of
    /// <see cref="ProviderOrderingDescriptor"/>.</param>
    /// <param name="setActualOrderOnly">If set to <see langword="true"/> then the actual order will be set 
    /// in <see cref="Provider.Header"/>. Other modifications will not be performed.</param>
    public OrderingCorrector(
      Func<CompilableProvider, ProviderOrderingDescriptor> orderingDescriptorResolver, bool setActualOrderOnly)
    {
      ArgumentValidator.EnsureArgumentNotNull(orderingDescriptorResolver, "orderingDescriptorResolver");
      this.orderingDescriptorResolver = orderingDescriptorResolver;
      this.setActualOrderOnly = setActualOrderOnly;
    }
  }
}