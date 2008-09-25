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

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Modification (insert, update, delete) request.
  /// </summary>
  public class SqlUpdateRequest : SqlRequest
  {
    /// <summary>
    /// Gets or sets the parameter bindings.
    /// </summary>
    public HashSet<SqlUpdateRequestParameter> Parameters { get; private set; }

    /// <summary>
    /// Gets or sets the expected result.
    /// </summary>
    /// <remarks>Usually is the number of touched rows.</remarks>
    public int ExpectedResult { get; set; }

    /// <summary>
    /// Binds the parameters to the specified <paramref name="target"/>.
    /// </summary>
    /// <param name="target">The target to bind parameters to.</param>
    public void BindParameters(Tuple target)
    {
      foreach (var binding in Parameters)
        binding.Parameter.Value = binding.Value.Invoke(target);
    }

    internal override void CompileWith(SqlDriver driver)
    {
      if (CompilationResult!=null)
        return;
      int i = 0;
      foreach (SqlUpdateRequestParameter binding in Parameters)
        binding.Parameter.ParameterName = "p" + i++;
      CompilationResult = driver.Compile(Statement);
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    public SqlUpdateRequest(ISqlCompileUnit statement)
      : base(statement)
    {
      Parameters = new HashSet<SqlUpdateRequestParameter>();
    }
  }
}