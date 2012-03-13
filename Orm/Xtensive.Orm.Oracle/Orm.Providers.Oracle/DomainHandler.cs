// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.04

using Xtensive.Orm.Rse.Compilation;

namespace Xtensive.Orm.Providers.Oracle
{
  /// <summary>
  /// A domain handler for Oracle RDBMS.
  /// </summary>
  public class DomainHandler : Providers.DomainHandler
  {
    protected override ICompiler CreateCompiler(CompilerConfiguration configuration)
    {
      return new SqlCompiler(Handlers);
    }
  }
}