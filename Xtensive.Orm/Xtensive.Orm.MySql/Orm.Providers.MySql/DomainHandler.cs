// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.03.25

using Xtensive.Sql.MySql;
using Xtensive.Storage.Rse.Compilation;

namespace Xtensive.Storage.Providers.Sql.Servers.MySql
{
  /// <summary>
  /// A domain handler for MySql RDBMS.
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