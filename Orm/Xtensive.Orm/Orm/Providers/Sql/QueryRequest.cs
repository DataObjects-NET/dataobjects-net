// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.22

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Orm.Providers.Sql.Resources;
using Xtensive.Sql.Compiler;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// Query (SELECT) request.
  /// </summary>
  public sealed class QueryRequest
  {
    private SqlCompilationResult compiledStatement;

    /// <summary>
    /// Gets SELECT statement.
    /// </summary>
    public SqlSelect Statement { get; private set; }

    /// <summary>
    /// Gets compiled SELECT statement.
    /// </summary>
    public SqlCompilationResult GetCompiledStatement()
    {
      if (compiledStatement==null)
        throw new InvalidOperationException(Strings.ExRequestIsNotPrepared);
      return compiledStatement;
    }

    /// <summary>
    /// Gets the parameter bindings.
    /// </summary>
    public IEnumerable<QueryParameterBinding> ParameterBindings { get; private set; }

    /// <summary>
    /// Gets the record set header.
    /// </summary>
    public TupleDescriptor TupleDescriptor { get; private set; }

    /// <summary>
    /// Gets the options of this request.
    /// </summary>
    public QueryRequestOptions Options { get; private set; }

    /// <summary>
    /// Checks that specified options are enabled for this request.
    /// </summary>
    /// <param name="requiredOptions">The required options.</param>
    /// <returns><see langword="true"/> is specified options is suppored;
    /// otherwise, <see langword="false"/>.</returns>
    public bool CheckOptions(QueryRequestOptions requiredOptions)
    {
      return (Options & requiredOptions)==requiredOptions;
    }

    /// <summary>
    /// Prepares this statement for execution.
    /// </summary>
    /// <param name="domainHandler">Domain handler to use.</param>
    public void Prepare(DomainHandler domainHandler)
    {
      if (compiledStatement!=null)
        return;
      compiledStatement = domainHandler.Driver.Compile(Statement);
      Statement = null;
    }

    // Constructors

    /// <summary>
    ///	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    /// <param name="tupleDescriptor">The tuple descriptor.</param>
    /// <param name="options">The options.</param>
    /// <param name="parameterBindings">The parameter bindings.</param>
    public QueryRequest(SqlSelect statement, TupleDescriptor tupleDescriptor, QueryRequestOptions options,
      IEnumerable<QueryParameterBinding> parameterBindings)
    {
      Statement = statement;
      TupleDescriptor = tupleDescriptor;
      Options = options;
      ParameterBindings = parameterBindings!=null
        ? parameterBindings.ToHashSet()
        : new HashSet<QueryParameterBinding>();
    }
  }
}