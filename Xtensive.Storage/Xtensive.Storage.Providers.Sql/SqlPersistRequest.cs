// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.22

using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Modification (insert, update, delete) request.
  /// </summary>
  public class SqlPersistRequest : SqlRequest
  {
    /// <summary>
    /// Gets the parameter bindings.
    /// </summary>
    public IEnumerable<SqlPersistParameterBinding> ParameterBindings { get; private set; }

    /// <summary>
    /// Gets or sets the expected result.
    /// </summary>
    /// <remarks>Usually is the number of touched rows.</remarks>
    public int? ExpectedResult { get; private set; }
    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    /// <param name="expectedResult">The expected result.</param>
    /// <param name="parameterBindings">The parameter bindings.</param>
    public SqlPersistRequest(ISqlCompileUnit statement, int? expectedResult,
      IEnumerable<SqlPersistParameterBinding> parameterBindings)
      : base(statement)
    {
      ExpectedResult = expectedResult;
      ParameterBindings = parameterBindings.ToHashSet();
    }
  }
}