// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.22

using System.Collections.Generic;
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
    public HashSet<SqlUpdateParameterBinding> ParameterBindings { get; private set; }

    /// <summary>
    /// Gets or sets the expected result.
    /// </summary>
    /// <remarks>Usually is the number of touched rows.</remarks>
    public int ExpectedResult { get; set; }

    /// <inheritdoc/>
    protected override IEnumerable<SqlParameterBinding> GetParameterBindings()
    {
      foreach (var binding in ParameterBindings)
        yield return binding;
    }

    /// <summary>
    /// Binds the parameters to the specified <paramref name="target"/>.
    /// </summary>
    /// <param name="target">The target to bind parameters to.</param>
    public void BindParameters(Tuple target)
    {
      foreach (var binding in ParameterBindings)
        BindParameter(binding, binding.ValueAccessor(target));
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    public SqlUpdateRequest(ISqlCompileUnit statement)
      : base(statement)
    {
      ParameterBindings = new HashSet<SqlUpdateParameterBinding>();
    }
  }
}