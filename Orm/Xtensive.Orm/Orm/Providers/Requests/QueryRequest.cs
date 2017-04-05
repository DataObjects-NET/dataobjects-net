// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.22

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;
using Xtensive.Tuples;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Query (SELECT) request.
  /// </summary>
  public sealed class QueryRequest : IQueryRequest
  {
    private readonly StorageDriver driver;

    private DbDataReaderAccessor accessor;
    private SqlCompilationResult compiledStatement;

    public SqlSelect Statement { get; private set; }
    public IEnumerable<QueryParameterBinding> ParameterBindings { get; private set; }

    public TupleDescriptor TupleDescriptor { get; private set; }
    public QueryRequestOptions Options { get; private set; }

    public NodeConfiguration NodeConfiguration { get; private set; }

    public bool CheckOptions(QueryRequestOptions requiredOptions)
    {
      return (Options & requiredOptions)==requiredOptions;
    }

    public void Prepare()
    {
      if (compiledStatement!=null && accessor!=null)
        return;
      compiledStatement = (NodeConfiguration!=null)? driver.Compile(Statement, NodeConfiguration) : driver.Compile(Statement);
      accessor = driver.GetDataReaderAccessor(TupleDescriptor);
      Statement = null;
    }

    public SqlCompilationResult GetCompiledStatement()
    {
      if (compiledStatement==null)
        throw new InvalidOperationException(Strings.ExRequestIsNotPrepared);
      return compiledStatement;
    }

    public DbDataReaderAccessor GetAccessor()
    {
      if (accessor==null)
        throw new InvalidOperationException(Strings.ExRequestIsNotPrepared);
      return accessor;
    }

    // Constructors

    public QueryRequest(
      StorageDriver driver, SqlSelect statement, IEnumerable<QueryParameterBinding> parameterBindings,
      TupleDescriptor tupleDescriptor, QueryRequestOptions options)
    {
      ArgumentValidator.EnsureArgumentNotNull(driver, "driver");
      ArgumentValidator.EnsureArgumentNotNull(statement, "statement");
      ArgumentValidator.EnsureArgumentNotNull(tupleDescriptor, "tupleDescriptor");

      this.driver = driver;
      Statement = statement;
      ParameterBindings = ParameterBinding.NormalizeBindings(parameterBindings);
      TupleDescriptor = tupleDescriptor;
      Options = options;
    }

    public QueryRequest(
      StorageDriver driver, SqlSelect statement, IEnumerable<QueryParameterBinding> parameterBindings,
      TupleDescriptor tupleDescriptor, QueryRequestOptions options, NodeConfiguration nodeConfiguration)
    {
      ArgumentValidator.EnsureArgumentNotNull(driver, "driver");
      ArgumentValidator.EnsureArgumentNotNull(statement, "statement");
      ArgumentValidator.EnsureArgumentNotNull(tupleDescriptor, "tupleDescriptor");

      this.driver = driver;
      Statement = statement;
      ParameterBindings = ParameterBinding.NormalizeBindings(parameterBindings);
      TupleDescriptor = tupleDescriptor;
      Options = options;
      NodeConfiguration = nodeConfiguration;
    }
  }
}