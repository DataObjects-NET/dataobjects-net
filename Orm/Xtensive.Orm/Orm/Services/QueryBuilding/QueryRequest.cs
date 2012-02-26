// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.26

using System.Collections.Generic;
using System.Linq;
using Xtensive.Orm.Providers.Sql;
using Xtensive.Sql.Compiler;

namespace Xtensive.Orm.Services
{
  public sealed class QueryRequest
  {
    public SqlCompilationResult CompiledQuery
    {
      get { return RealRequest.GetCompiledStatement(); }
    }

    public IEnumerable<QueryParameterBinding> ParameterBindings
    {
      get { return RealRequest.ParameterBindings.Select(b => new QueryParameterBinding(b)); }
    }

    internal UserQueryRequest RealRequest { get; private set; }

    // Constructors

    internal QueryRequest(UserQueryRequest realRequest)
    {
      this.RealRequest = realRequest;
    }
  }
}