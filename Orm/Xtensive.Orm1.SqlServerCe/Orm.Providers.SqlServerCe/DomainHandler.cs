// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.07.04

using Xtensive.Orm.Rse.Compilation;

namespace Xtensive.Orm.Providers.SqlServerCe
{
  /// <summary>
  /// A domain handler specific to Microsoft SQL Server Compact Edition RDBMS.
  /// </summary>
  public class DomainHandler : Providers.DomainHandler
  {
    protected override ICompiler CreateCompiler(CompilerConfiguration configuration)
    {
      return new SqlCompiler(Handlers, configuration);
    }
  }
}