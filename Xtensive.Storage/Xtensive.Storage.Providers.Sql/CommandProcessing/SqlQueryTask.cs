// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.21

using System.Collections.Generic;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Providers.Sql
{
  internal sealed class SqlQueryTask : SqlTask
  {
    public SqlQueryRequest Request;
    public ParameterContext ParameterContext;
    public List<Tuple> Result;
    
    // Constructors

    public SqlQueryTask(SqlQueryRequest request)
    {
      Request = request;
      ParameterContext = null;
      Result = null;
    }

    public SqlQueryTask(SqlQueryRequest request, ParameterContext parameterContext, List<Tuple> output)
    {
      Request = request;
      ParameterContext = parameterContext;
      Result = output;
    }
  }
}