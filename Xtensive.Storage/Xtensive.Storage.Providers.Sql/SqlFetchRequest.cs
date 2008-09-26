// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.22

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dom;
using Xtensive.Storage.Rse;

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
    public HashSet<SqlFetchParameterBinding> ParameterBindings { get; private set; }

    /// <summary>
    /// Gets the result element descriptor.
    /// </summary>
    public TupleDescriptor TupleDescriptor { get; private set; }

    /// <summary>
    /// Gets the record set header.
    /// </summary>
    public RecordSetHeader RecordSetHeader { get; private set; }

    /// <summary>
    /// Binds the parameters to actual values.
    /// </summary>
    public void BindParameters()
    {
      foreach (var binding in ParameterBindings)
        binding.Parameter.Value = binding.Value();
    }

    internal override void CompileWith(SqlDriver driver)
    {
      if (CompilationResult!=null)
        return;

      int i = 0;
      foreach (SqlFetchParameterBinding binding in ParameterBindings)
        binding.Parameter.ParameterName = "p" + i++;
      CompilationResult = driver.Compile(Statement);

      TupleDescriptor = TupleDescriptor.Create(RecordSetHeader.Columns.Select(c => c.Type));
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    /// <param name="recordSetHeader">The element descriptor.</param>
    public SqlFetchRequest(ISqlCompileUnit statement, RecordSetHeader recordSetHeader)
      : base(statement)
    {
      ParameterBindings = new HashSet<SqlFetchParameterBinding>();
      RecordSetHeader = recordSetHeader;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    /// <param name="recordSetHeader">The element descriptor.</param>
    /// <param name="parameterBindings">The parameter bindings.</param>
    public SqlFetchRequest(ISqlCompileUnit statement, RecordSetHeader recordSetHeader, IEnumerable<SqlFetchParameterBinding> parameterBindings)
      : this(statement, recordSetHeader)
    {
      ParameterBindings.UnionWith(parameterBindings);
    }

  }
}