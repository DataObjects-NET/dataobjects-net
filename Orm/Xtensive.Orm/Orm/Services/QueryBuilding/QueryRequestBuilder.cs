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
  /// <summary>
  /// Builder of <see cref="QueryRequest"/>s.
  /// </summary>
  public sealed class QueryRequestBuilder
  {
    private readonly StorageDriver driver;

    /// <summary>
    /// Gets or sets SQL DOM query.
    /// </summary>
    public ISqlCompileUnit Query { get; set; }

    /// <summary>
    /// Gets list of all available <see cref="QueryParameterBinding"/>s
    /// that are to be associated with request.
    /// </summary>
    public IList<QueryParameterBinding> ParameterBindings { get; private set; }

    /// <summary>
    /// Creates new <see cref="QueryParameterBinding"/> with specified
    /// <paramref name="valueType"/> and <paramref name="valueAccessor"/>
    /// and adds it to <see cref="ParameterBindings"/>.
    /// </summary>
    /// <param name="valueType">Value type to use.</param>
    /// <param name="valueAccessor">Value accessor to use.</param>
    /// <returns>Created binding.</returns>
    public QueryParameterBinding AddParameterBinding(Type valueType, Func<object> valueAccessor)
    {
      var realBinding = new Providers.Sql.QueryParameterBinding(valueAccessor, driver.GetTypeMapping(valueType));
      var result = new QueryParameterBinding(realBinding);
      ParameterBindings.Add(result);
      return result;
    }

    /// <summary>
    /// Builds request using current <see cref="Query"/>
    /// and <see cref="ParameterBindings"/>.
    /// </summary>
    /// <returns>Built request.</returns>
    public QueryRequest BuildRequest()
    {
      if (Query==null)
        throw new InvalidOperationException("Query is not set");

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