// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.20

using System;
using System.Collections.Generic;
using Xtensive.IoC;
using Xtensive.Orm.Providers.Sql.Expressions;
using Xtensive.Orm.Rse.Compilation;
using Xtensive.Orm.Rse.PreCompilation.Correction;
using Xtensive.Orm.Rse.PreCompilation.Correction.ApplyProviderCorrection;
using Xtensive.Orm.Rse.PreCompilation.Optimization;
using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// <see cref="Orm.Domain"/>-level handler.
  /// </summary>
  public abstract class DomainHandler : Providers.DomainHandler
  {
    /// <inheritdoc/>
    protected override IEnumerable<Type> GetProviderCompilerContainers()
    {
      return new[] {
        typeof (NullableCompilers),
        typeof (StringCompilers),
        typeof (DateTimeCompilers),
        typeof (TimeSpanCompilers),
        typeof (MathCompilers),
        typeof (NumericCompilers),
        typeof (DecimalCompilers),
        typeof (GuidCompilers),
        typeof (VbStringsCompilers),
        typeof (VbDateAndTimeCompilers),
      };
    }

    /// <inheritdoc/>
    protected override ICompiler CreateCompiler(CompilerConfiguration configuration)
    {
      return new SqlCompiler(Handlers);
    }

    /// <inheritdoc/>
    protected override IPostCompiler CreatePostCompiler(CompilerConfiguration configuration, ICompiler compiler)
    {
      var result = new CompositePostCompiler(new SqlSelectCorrector());
      if (configuration.PrepareRequest)
        result.Items.Add(new SqlProviderPreparer(Handlers));
      return result;
    }

    private static ProviderOrderingDescriptor ResolveOrderingDescriptor(CompilableProvider provider)
    {
      bool isOrderSensitive = provider.Type==ProviderType.Skip 
        || provider.Type == ProviderType.Take
        || provider.Type == ProviderType.Seek
        || provider.Type == ProviderType.Paging
        || provider.Type == ProviderType.RowNumber;
      bool preservesOrder = provider.Type==ProviderType.Take
        || provider.Type == ProviderType.Skip
        || provider.Type == ProviderType.Seek
        || provider.Type == ProviderType.RowNumber
        || provider.Type == ProviderType.Paging
        || provider.Type == ProviderType.Distinct
        || provider.Type == ProviderType.Alias;
      bool isOrderBreaker = provider.Type == ProviderType.Except
        || provider.Type == ProviderType.Intersect
        || provider.Type == ProviderType.Union
        || provider.Type == ProviderType.Concat
        || provider.Type == ProviderType.Existence;
      bool isSorter = provider.Type==ProviderType.Sort || provider.Type == ProviderType.Index;
      return new ProviderOrderingDescriptor(isOrderSensitive, preservesOrder, isOrderBreaker, isSorter);
    }

    /// <inheritdoc/>
    protected override IPreCompiler CreatePreCompiler(CompilerConfiguration configuration)
    {
      var providerInfo = Handlers.ProviderInfo;

      var applyCorrector = new ApplyProviderCorrector(
        !providerInfo.Supports(ProviderFeatures.Apply));
      var skipTakeCorrector = new SkipTakeCorrector(
        providerInfo.Supports(ProviderFeatures.NativeTake),
        providerInfo.Supports(ProviderFeatures.NativeSkip));
      return new CompositePreCompiler(
        applyCorrector,
        skipTakeCorrector,
        new RedundantColumnOptimizer(),
        new OrderingCorrector(ResolveOrderingDescriptor));
    }

    /// <inheritdoc/>
    protected override void AddBaseServiceRegistrations(List<ServiceRegistration> registrations)
    {
      registrations.Add(new ServiceRegistration(typeof (ICachingKeyGeneratorService), new CachingKeyGeneratorService(Handlers)));
    }
  }
}