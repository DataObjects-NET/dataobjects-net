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
  /// Fetch request.
  /// </summary>
  public class SqlFetchRequest : SqlRequest
  {
    /// <summary>
    /// Gets or sets the parameter bindings.
    /// </summary>
    public Dictionary<SqlParameter, Func<object>> ParameterBindings { get; private set; }

    /// <summary>
    /// Gets or sets the result element descriptor.
    /// </summary>
    public TupleDescriptor ElementDescriptor { get; private set; }

    /// <inheritdoc/>
    public override List<SqlParameter> GetParameters()
    {
      return ParameterBindings.Keys.ToList();
    }

    /// <summary>
    /// Binds the parameters to actual values.
    /// </summary>
    public void BindParameters()
    {
      foreach (KeyValuePair<SqlParameter, Func<object>> binding in ParameterBindings)
        binding.Key.Value = binding.Value.Invoke();
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    /// <param name="elementDescriptor">The element descriptor.</param>
    public SqlFetchRequest(ISqlCompileUnit statement, TupleDescriptor elementDescriptor)
      : base(statement)
    {
      ParameterBindings = new Dictionary<SqlParameter, Func<object>>();
      ElementDescriptor = elementDescriptor;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    /// <param name="elementDescriptor">The element descriptor.</param>
    /// <param name="parameterBindings">The parameter bindings.</param>
    public SqlFetchRequest(ISqlCompileUnit statement, TupleDescriptor elementDescriptor, IEnumerable<KeyValuePair<SqlParameter, Func<object>>> parameterBindings)
      : this(statement, elementDescriptor)
    {
      foreach (var binding in parameterBindings)
        ParameterBindings.Add(binding.Key, binding.Value);
    }

  }
}