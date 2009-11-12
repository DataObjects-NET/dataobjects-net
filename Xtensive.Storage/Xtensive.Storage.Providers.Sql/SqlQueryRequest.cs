// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.22

using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dml;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Fetch request.
  /// </summary>
  public class SqlQueryRequest : SqlRequest
  {
    /// <summary>
    /// Gets the select statement.
    /// </summary>
    /// <value>The select statement.</value>
    public SqlSelect SelectStatement { get { return (SqlSelect) Statement; } }

    /// <summary>
    /// Gets the parameter bindings.
    /// </summary>
    public IEnumerable<SqlQueryParameterBinding> ParameterBindings { get; private set; }

    /// <summary>
    /// Gets the record set header.
    /// </summary>
    public TupleDescriptor TupleDescriptor { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether batching of this query is allowed.
    /// </summary>
    public bool AllowBatching { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    /// <param name="tupleDescriptor">The tuple descriptor.</param>
    /// <param name="allowBatching">if set to <see langword="true"/> batching of this query is allowed.</param>
    public SqlQueryRequest(SqlSelect statement, TupleDescriptor tupleDescriptor, bool allowBatching)
      : this(statement, tupleDescriptor, allowBatching, EnumerableUtils<SqlQueryParameterBinding>.Empty)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    /// <param name="tupleDescriptor">The tuple descriptor.</param>
    /// <param name="allowBatching">if set to <see langword="true"/> batching of this query is allowed.</param>
    /// <param name="parameterBindings">The parameter bindings.</param>
    public SqlQueryRequest(SqlSelect statement, TupleDescriptor tupleDescriptor,
      bool allowBatching, IEnumerable<SqlQueryParameterBinding> parameterBindings)
      : base(statement)
    {
      ParameterBindings = parameterBindings.ToHashSet();
      TupleDescriptor = tupleDescriptor;
      AllowBatching = allowBatching;
    }
  }
}