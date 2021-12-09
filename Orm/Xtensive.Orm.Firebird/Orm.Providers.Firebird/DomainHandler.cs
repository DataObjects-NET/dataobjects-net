// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.01.27

using Xtensive.Orm.Rse.Compilation;

namespace Xtensive.Orm.Providers.Firebird
{
  /// <summary>
  /// A domain handler for Firebird RDBMS.
  /// </summary>
  public class DomainHandler : Providers.DomainHandler
  {
    /// <inheritdoc/>
    protected override ICompiler CreateCompiler(CompilerConfiguration configuration)
    {
      return new SqlCompiler(Handlers, configuration);
    }
  }
}