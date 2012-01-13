// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.04

using Xtensive.Sql.Drivers.Oracle;
using Xtensive.Orm.Rse.Compilation;

namespace Xtensive.Orm.Providers.Oracle
{
  /// <summary>
  /// A domain handler for Oracle RDBMS.
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