// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.07.04

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Rse.Compilation;

namespace Xtensive.Orm.Providers.SqlServer
{
  /// <summary>
  /// A domain handler specific to Microsoft SQL Server RDBMS.
  /// </summary>
  public class DomainHandler : Providers.DomainHandler
  {
    /// <inheritdoc/>
    protected override ICompiler CreateCompiler(CompilerConfiguration configuration)
    {
      return new SqlCompiler(Handlers);
    }
  }
}