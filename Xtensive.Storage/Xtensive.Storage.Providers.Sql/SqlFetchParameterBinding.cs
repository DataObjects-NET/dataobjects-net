// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.25

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql.ValueTypeMapping;

namespace Xtensive.Storage.Providers.Sql
{
  public sealed class SqlFetchParameterBinding : SqlParameterBinding
  {
    /// <summary>
    /// Gets the value accessor.
    /// </summary>
    public Func<object> ValueAccessor { get; private set; }

    /// <summary>
    /// If set to true and <see cref="ValueAccessor"/> returns <see langword="null"/>
    /// generated query with contain "something is null" check instead of "something = @p".
    /// </summary>
    public bool SmartNull { get; private set; }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="valueAccessor">Value for <see cref="ValueAccessor"/>.</param>
    /// <param name="typeMapping">Value for <see cref="SqlParameterBinding.TypeMapping"/>.</param>
    /// <param name="smartNull">Value for <see cref="SmartNull"/>.</param>
    public SqlFetchParameterBinding(Func<object> valueAccessor, TypeMapping typeMapping, bool smartNull)
      : base(typeMapping)
    {
      ValueAccessor = valueAccessor;
      SmartNull = smartNull;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>.
    /// </summary>
    /// <param name="valueAccessor"></param>
    /// <param name="typeMapping"></param>
    public SqlFetchParameterBinding(Func<object> valueAccessor, TypeMapping typeMapping)
      : this(valueAccessor, typeMapping, false)
    {
    }
  }
}
