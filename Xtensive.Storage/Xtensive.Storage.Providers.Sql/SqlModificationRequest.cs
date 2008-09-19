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
  public class SqlModificationRequest : SqlRequest
  {
    /// <summary>
    /// Gets or sets the parameter bindings.
    /// </summary>
    public Dictionary<SqlParameter, Func<Tuple, object>> ParameterBindings { get; private set; }

    /// <summary>
    /// Gets or sets the expected result.
    /// </summary>
    /// <remarks>Usually is the number of touched rows.</remarks>
    public int ExpectedResult { get; set; }

    /// <inheritdoc/>
    public override List<SqlParameter> GetParameters()
    {
      return ParameterBindings.Keys.ToList();
    }

    /// <summary>
    /// Binds the parameters to the specified <paramref name="target"/>.
    /// </summary>
    /// <param name="target">The target to bind parameters to.</param>
    public void BindParametersTo(Tuple target)
    {
      foreach (KeyValuePair<SqlParameter, Func<Tuple, object>> binding in ParameterBindings)
        binding.Key.Value = binding.Value.Invoke(target);
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    public SqlModificationRequest(ISqlCompileUnit statement)
      : base(statement)
    {
      ParameterBindings = new Dictionary<SqlParameter, Func<Tuple, object>>();
    }
  }
}