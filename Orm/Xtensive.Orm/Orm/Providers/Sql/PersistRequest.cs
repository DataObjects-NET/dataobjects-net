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
using Xtensive.Sql;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// Modification (Insert, Update, Delete) request.
  /// </summary>
  public class PersistRequest : Request
  {
    /// <summary>
    /// Gets the parameter bindings.
    /// </summary>
    public IEnumerable<PersistParameterBinding> ParameterBindings { get; private set; }
    

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    public PersistRequest(ISqlCompileUnit statement)
      : this(statement, EnumerableUtils<PersistParameterBinding>.Empty)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="statement">The statement.</param>
    /// <param name="parameterBindings">The parameter bindings.</param>
    public PersistRequest(ISqlCompileUnit statement, IEnumerable<PersistParameterBinding> parameterBindings)
      : base(statement)
    {
      ParameterBindings = parameterBindings.ToHashSet();
    }
  }
}