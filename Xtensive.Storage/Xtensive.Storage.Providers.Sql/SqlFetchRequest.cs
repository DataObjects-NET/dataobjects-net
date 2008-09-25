// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.22

using System;
using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dom;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Fetch request.
  /// </summary>
  public class SqlFetchRequest : SqlRequest
  {
    /// <summary>
    /// Gets or sets the parameter bindings.
    /// </summary>
    public HashSet<SqlFetchRequestParameter> Parameters { get; private set; }

    /// <summary>
    /// Gets or sets the result element descriptor.
    /// </summary>
    public TupleDescriptor TupleDescriptor { get; private set; }

    /// <summary>
    /// Binds the parameters to actual values.
    /// </summary>
    public void BindParameters()
    {
      foreach (var binding in Parameters)
        binding.Parameter.Value = binding.Value.Invoke();
    }

    internal override void CompileWith(SqlDriver driver)
    {
      if (CompilationResult!=null)
        return;
      int i = 0;
      foreach (SqlFetchRequestParameter binding in Parameters)
        binding.Parameter.ParameterName = "p" + i++;
      CompilationResult = driver.Compile(Statement);
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    /// <param name="tupleDescriptor">The element descriptor.</param>
    public SqlFetchRequest(ISqlCompileUnit statement, TupleDescriptor tupleDescriptor)
      : base(statement)
    {
      Parameters = new HashSet<SqlFetchRequestParameter>();
      TupleDescriptor = tupleDescriptor;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    /// <param name="tupleDescriptor">The element descriptor.</param>
    /// <param name="parameterBindings">The parameter bindings.</param>
    public SqlFetchRequest(ISqlCompileUnit statement, TupleDescriptor tupleDescriptor, IEnumerable<SqlFetchRequestParameter> parameterBindings)
      : this(statement, tupleDescriptor)
    {
      Parameters.UnionWith(parameterBindings);
    }

  }
}