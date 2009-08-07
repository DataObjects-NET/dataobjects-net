// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.04

using Xtensive.Core.Collections;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.Sql.Servers.Oracle
{
  /// <summary>
  /// A domain handler for Oracle RDBMS.
  /// </summary>
  public class DomainHandler : Sql.DomainHandler
  {
    /// <inheritdoc/>
    protected override ICompiler CreateCompiler(BindingCollection<object, ExecutableProvider> compiledSources)
    {
      return new SqlCompiler(Handlers, compiledSources);
    }
  }
}