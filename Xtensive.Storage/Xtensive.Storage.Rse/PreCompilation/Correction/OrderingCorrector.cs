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
using Xtensive.Storage.Rse.PreCompilation.Optimization;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.PreCompilation.Correction
{
  /// <summary>
  /// Order by <see cref="IPreCompiler"/> implementation.
  /// </summary>
  [Serializable]
  public sealed class OrderingCorrector : IPreCompiler
  {
    private readonly OrderingCorrectionRewriter rewriter;

    /// <inheritdoc/>
    CompilableProvider IPreCompiler.Process(CompilableProvider rootProvider)
    {
      return rewriter.Rewrite(rootProvider);
    }


    // Constructors

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="orderingDescriptorResolver">The resolver of
    /// <see cref="ProviderOrderingDescriptor"/>.</param>
    /// <param name="setActualOrderOnly">If set to <see langword="true"/> then the actual order will be set 
    /// in <see cref="Provider.Header"/>. Other modifications will not be performed.</param>
    public OrderingCorrector(
      Func<CompilableProvider, ProviderOrderingDescriptor> orderingDescriptorResolver, bool setActualOrderOnly)
    {
      ArgumentValidator.EnsureArgumentNotNull(orderingDescriptorResolver, "orderingDescriptorResolver");
      rewriter = new OrderingCorrectionRewriter(orderingDescriptorResolver, setActualOrderOnly);
    }
  }
}