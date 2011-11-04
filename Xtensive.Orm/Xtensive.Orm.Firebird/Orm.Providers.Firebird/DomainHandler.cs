// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.01.27

using System;
using Xtensive.Storage.Rse.Compilation;

namespace Xtensive.Storage.Providers.Sql.Servers.Firebird
{
  /// <summary>
  /// A domain handler for Firebird RDBMS.
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