// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.07.04

using Xtensive.Core.Collections;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.Sql.Servers.PostgreSql
{
  /// <summary>
  /// A domain handler specific to PostgreSQL RDBMS.
  /// </summary>
  public class DomainHandler : Sql.DomainHandler
  {
    /// <inheritdoc/>
    protected override ICompiler CreateCompiler()
    {
      return new SqlCompiler(Handlers);
    }
  }
}