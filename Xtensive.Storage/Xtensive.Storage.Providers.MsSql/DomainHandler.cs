// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.07.04

using System;
using Xtensive.Sql.Common;
using Xtensive.Storage.Providers.MsSql.Resources;
using Xtensive.Storage.Rse.Compilation;

namespace Xtensive.Storage.Providers.MsSql
{
  public class DomainHandler : Sql.DomainHandler
  {
    protected override ICompiler BuildCompiler()
    {
      return new MsSqlCompiler(Handlers);
    }
  }
}