// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.03.25

using Xtensive.Sql.Drivers.Sqlite;
using Xtensive.Orm.Rse.Compilation;

namespace Xtensive.Orm.Providers.Sqlite
{
  /// <summary>
  /// A domain handler for Sqlite RDBMS.
  /// </summary>
  public class DomainHandler : Providers.DomainHandler
  {
    protected override ICompiler CreateCompiler(CompilerConfiguration configuration)
    {
      return new SqlCompiler(Handlers, configuration);
    }
  }
}