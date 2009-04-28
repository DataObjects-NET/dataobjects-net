// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.09.01

using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.PreCompilation;
using Xtensive.Storage.Rse.PreCompilation.Correction;
using Xtensive.Storage.Rse.PreCompilation.Optimization;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// Default <see cref="CompilationContext"/> implementation.
  /// </summary>
  /// <remarks>
  /// <para id="About"><see cref="HasStaticDefaultDocTemplate" copy="true" /></para>
  /// </remarks>
  public sealed class DefaultCompilationContext : CompilationContext
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
        || (asJoin!=null && asJoin.JoinType==JoinType.Merge);
      bool isOrderingBoundary = provider.Type==ProviderType.Except
        || provider.Type==ProviderType.Intersect || provider.Type==ProviderType.Union
        || provider.Type==ProviderType.Concat || provider.Type==ProviderType.Existence;
      return new ProviderOrderingDescriptor(isOrderSensitive, true, isOrderingBoundary);
    }

    /// <inheritdoc/>
    public override EnumerationContext CreateEnumerationContext()
    {
      return new DefaultEnumerationContext();
    }


    // Constructors

    /// <inheritdoc/>
    public DefaultCompilationContext()
      : base(() => {
        var compiledSource = new BindingCollection<object, ExecutableProvider>();
        return new ManagingCompiler(compiledSource, new ClientCompiler(compiledSource));
      }, () => new CompositePreCompiler(new OrderingCorrector(ResolveOrderingDescriptor, false)))
    {
    }
  }
}