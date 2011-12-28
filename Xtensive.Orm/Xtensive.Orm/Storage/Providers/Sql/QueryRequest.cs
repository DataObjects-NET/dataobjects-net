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
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Query request.
  /// </summary>
  public class QueryRequest : Request
  {
    /// <summary>
    /// Gets the select statement.
    /// </summary>
    /// <value>The select statement.</value>
    public SqlSelect SelectStatement { get { return (SqlSelect) Statement; } }

    /// <summary>
    /// Gets the parameter bindings.
    /// </summary>
    public IEnumerable<QueryParameterBinding> ParameterBindings { get; private set; }

    /// <summary>
    /// Gets the record set header.
    /// </summary>
    public TupleDescriptor TupleDescriptor { get; private set; }


    // Constructors

    /// <summary>
    ///	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    /// <param name="tupleDescriptor">The tuple descriptor.</param>
    /// <param name="options">The options.</param>
    public QueryRequest(ISqlCompileUnit statement, TupleDescriptor tupleDescriptor, RequestOptions options)
      : this(statement, tupleDescriptor, options, EnumerableUtils<QueryParameterBinding>.Empty)
    {
    }

    /// <summary>
    ///	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    /// <param name="tupleDescriptor">The tuple descriptor.</param>
    /// <param name="options">The options.</param>
    /// <param name="parameterBindings">The parameter bindings.</param>
    public QueryRequest(ISqlCompileUnit statement, TupleDescriptor tupleDescriptor, RequestOptions options, IEnumerable<QueryParameterBinding> parameterBindings)
      : base(statement, options)
    {
      ParameterBindings = parameterBindings.ToHashSet();
      TupleDescriptor = tupleDescriptor;
    }
  }
}