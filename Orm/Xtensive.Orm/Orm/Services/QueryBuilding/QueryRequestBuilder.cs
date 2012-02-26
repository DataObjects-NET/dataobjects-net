// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.26

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Orm.Providers.Sql;
using Xtensive.Sql;

namespace Xtensive.Orm.Services
{
  public sealed class QueryRequestBuilder
  {
    private readonly StorageDriver driver;

    public ISqlCompileUnit Query { get; set; }

    public IList<QueryParameterBinding> ParameterBindings { get; private set; }

    public QueryParameterBinding AddParameterBinding(Type valueType, Func<object> valueAccessor)
    {
      var realBinding = new Providers.Sql.QueryParameterBinding(valueAccessor, driver.GetTypeMapping(valueType));
      var result = new QueryParameterBinding(realBinding);
      ParameterBindings.Add(result);
      return result;
    }

    public QueryRequest BuildRequest()
    {
      if (Query==null)
        throw new InvalidOperationException();

      var compiledStatement = driver.Compile(Query);
      var bindings = ParameterBindings.Select(b => b.RealBinding);

      return new QueryRequest(new UserQueryRequest(compiledStatement, bindings));
    }

    // Constructors

    internal QueryRequestBuilder(StorageDriver driver)
    {
      this.driver = driver;

      ParameterBindings = new List<QueryParameterBinding>();
    }
  }
}