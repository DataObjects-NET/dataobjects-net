// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.07.04

using Xtensive.Sql.SqlServerCe;
using Xtensive.Storage.Rse.Compilation;

namespace Xtensive.Orm.Providers.SqlServerCe
{
  /// <summary>
  /// A domain handler specific to Microsoft SQL Server Compact Edition RDBMS.
  /// </summary>
  public class DomainHandler : Sql.DomainHandler
  {
    protected override Xtensive.Sql.SqlDriverFactory GetDriverFactory()
    {
      return new DriverFactory();
    }

    /// <inheritdoc/>
    protected override ICompiler CreateCompiler()
    {
      return new SqlCompiler(Handlers);
    }
  }
}