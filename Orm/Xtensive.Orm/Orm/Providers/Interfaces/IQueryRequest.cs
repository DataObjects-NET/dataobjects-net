// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.25

using System.Collections.Generic;
using Xtensive.Sql.Compiler;

namespace Xtensive.Orm.Providers
{
  public interface IQueryRequest
  {
    IEnumerable<QueryParameterBinding> ParameterBindings { get; }

    SqlCompilationResult GetCompiledStatement();
  }
}