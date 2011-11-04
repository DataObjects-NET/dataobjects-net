// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.01.27

using System;
using System.Linq;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.Sql.Servers.Firebird
{
  internal class SqlCompiler : Sql.SqlCompiler
  {
    protected override SqlProvider VisitInclude(IncludeProvider provider)
    {
      var newProvider = new IncludeProvider(provider.Source, IncludeAlgorithm.ComplexCondition, provider.IsInlined,
                                            provider.FilterDataSource, provider.ResultColumnName,
                                            provider.FilteredColumns);
      return base.VisitInclude(newProvider);
    }

    // Constructors
    
    public SqlCompiler(HandlerAccessor handlers)
      : base(handlers)
    {
    }
  }
}