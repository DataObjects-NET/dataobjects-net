// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.09.01

using Xtensive.Collections;
using Xtensive.Internals.DocTemplates;
using Xtensive.Storage.Rse.PreCompilation;
using Xtensive.Storage.Rse.PreCompilation.Correction;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// Default <see cref="CompilationService"/> implementation.
  /// </summary>
  public sealed class DefaultCompilationService : CompilationService
  {
    /// <summary>
    /// Default method to resolve <see cref="ProviderOrderingDescriptor"/> 
    /// for <paramref name="provider"/>.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <returns>Resolved <see cref="ProviderOrderingDescriptor"/>.</returns>
    public static ProviderOrderingDescriptor ResolveOrderingDescriptor(CompilableProvider provider)
    {
      var asJoin = provider as JoinProvider;
      bool isOrderSensitive = provider.Type==ProviderType.Skip || provider.Type==ProviderType.Take
        || (asJoin!=null && asJoin.JoinAlgorithm==JoinAlgorithm.Merge)
        || provider.Type==ProviderType.Seek || provider.Type==ProviderType.RowNumber;
      bool isOrderBreaker = provider.Type==ProviderType.Except
        || provider.Type==ProviderType.Intersect || provider.Type==ProviderType.Union
        || provider.Type==ProviderType.Concat || provider.Type==ProviderType.Existence
        || provider.Type==ProviderType.Distinct;
      bool isSorter = provider.Type==ProviderType.Sort || provider.Type==ProviderType.Reindex;
      return new ProviderOrderingDescriptor(isOrderSensitive, true, isOrderBreaker, isSorter);
    }


    // Constructors

    /// <inheritdoc/>
    public DefaultCompilationService()
      : base(
        () => new ClientCompiler(),
        () => new CompositePreCompiler(new OrderingCorrector(ResolveOrderingDescriptor)),
        (compiler) => new EmptyPostCompiler())
    {
    }
  }
}