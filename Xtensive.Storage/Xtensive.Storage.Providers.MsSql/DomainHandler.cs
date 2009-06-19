// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.07.04

using Xtensive.Core.Collections;
using Xtensive.Storage.Building;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.PreCompilation;
using Xtensive.Storage.Rse.PreCompilation.Correction;
using Xtensive.Storage.Rse.PreCompilation.Correction.ApplyProviderCorrection;
using Xtensive.Storage.Rse.PreCompilation.Optimization;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.MsSql
{
  /// <summary>
  /// A domain handler specific to MSSql RDBMS.
  /// </summary>
  public class DomainHandler : Sql.DomainHandler
  {
    /// <inheritdoc/>
    protected override IPreCompiler CreatePreCompiler()
    {
      return new CompositePreCompiler(
        new ApplyProviderCorrector(false),
        new SkipTakeCorrector(),
        new OrderingCorrector(ResolveOrderingDescriptor, false),
        new RedundantColumnOptimizer(),
        new OrderingCorrector(ResolveOrderingDescriptor, true)
        );
    }

    /// <inheritdoc/>
    protected override ICompiler CreateCompiler(BindingCollection<object, ExecutableProvider> compiledSources)
    {
      return new MsSqlCompiler(Handlers, compiledSources);
    }

    /// <inheritdoc/>
    protected override ProviderInfo CreateProviderInfo()
    {
      var serverInfo = ((SessionHandler) BuildingContext.Current.SystemSessionHandler).Connection
        .Driver.ServerInfo;
      return new ProviderInfo(serverInfo, false, false, true) {SupportsRealTimeSpan = false};
    }
  }
}