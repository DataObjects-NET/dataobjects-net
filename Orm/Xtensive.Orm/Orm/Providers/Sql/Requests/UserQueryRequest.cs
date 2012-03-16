// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.25

using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Sql.Compiler;

namespace Xtensive.Orm.Providers.Sql
{
  public sealed class UserQueryRequest : IQueryRequest
  {
    private readonly SqlCompilationResult compiledStatement;

    public SqlCompilationResult GetCompiledStatement()
    {
      return compiledStatement;
    }

    public IEnumerable<QueryParameterBinding> ParameterBindings { get; private set; }

    // Constructors

    public UserQueryRequest(SqlCompilationResult compiledStatement, IEnumerable<QueryParameterBinding> parameterBindings)
    {
      ArgumentValidator.EnsureArgumentNotNull(compiledStatement, "compiledStatement");
      ArgumentValidator.EnsureArgumentNotNull(parameterBindings, "parameterBindings");

      this.compiledStatement = compiledStatement;
      ParameterBindings = ParameterBinding.NormalizeBindings(parameterBindings);
    }
  }
}