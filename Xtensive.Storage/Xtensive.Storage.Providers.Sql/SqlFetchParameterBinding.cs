// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.25

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Providers.Sql
{
  public sealed class SqlFetchParameterBinding : SqlParameterBinding
  {
    /// <summary>
    /// Gets the column reference.
    /// </summary>
    public ColumnInfoRef ColumnRef { get; private set; }

    /// <summary>
    /// Gets the value accessor.
    /// </summary>
    public Func<object> ValueAccessor { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="column">The column reference.</param>
    /// <param name="valueAccessor">The value accessor.</param>
    public SqlFetchParameterBinding(ColumnInfoRef column, Func<object> valueAccessor)
      : this(valueAccessor)
    {
      ColumnRef = column;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="valueAccessor">The value accessor.</param>
    public SqlFetchParameterBinding(Func<object> valueAccessor)
    {
      ValueAccessor = valueAccessor;
    }
  }
}