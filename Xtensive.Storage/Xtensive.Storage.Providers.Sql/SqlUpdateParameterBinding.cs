// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.25

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Sql.ValueTypeMapping;

namespace Xtensive.Storage.Providers.Sql
{
  public sealed class SqlUpdateParameterBinding : SqlParameterBinding
  {
    public Func<Tuple, object> ValueAccessor { get; private set; }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="typeMapping">The type mapping.</param>
    /// <param name="valueAccessor">The value accessor.</param>
    public SqlUpdateParameterBinding(Func<Tuple, object> valueAccessor, TypeMapping typeMapping)
      : base(typeMapping)
    {
      ValueAccessor = valueAccessor;
    }
  }
}