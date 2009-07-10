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
  public class SqlFetchRequest : SqlRequest
  {
    /// <summary>
    /// Gets the select statement.
    /// </summary>
    /// <value>The select statement.</value>
    public SqlSelect SelectStatement { get { return (SqlSelect) Statement; } }

    /// <summary>
    /// Gets the parameter bindings.
    /// </summary>
    public IEnumerable<SqlFetchParameterBinding> ParameterBindings { get; private set; }

    /// <summary>
    /// Gets the record set header.
    /// </summary>
    public TupleDescriptor TupleDescriptor { get; private set; }


    // Constructors

    /// <summary>
    ///  <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    /// <param name="tupleDescriptor">The tuple descriptor.</param>
    /// <param name="parameterBindings">The parameter bindings.</param>
    public SqlFetchRequest(SqlSelect statement, TupleDescriptor tupleDescriptor, IEnumerable<SqlFetchParameterBinding> parameterBindings)
      : base(statement)
    {
      ParameterBindings = parameterBindings.ToHashSet();
      TupleDescriptor = tupleDescriptor;
    }
  }
}