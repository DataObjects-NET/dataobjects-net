// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.07.04

using Xtensive.Sql.Drivers.PostgreSql;
using Xtensive.Orm.Rse.Compilation;

namespace Xtensive.Orm.Providers.PostgreSql
{
  /// <summary>
  /// A domain handler specific to PostgreSQL RDBMS.
  /// </summary>
  public class DomainHandler : Sql.DomainHandler
  {
    protected override Xtensive.Sql.SqlDriverFactory GetDriverFactory()
    {
      return new DriverFactory();
    }

    protected override ICompiler CreateCompiler(CompilerConfiguration configuration)
    {
      return new SqlCompiler(Handlers);
    }
  }
}