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
    /// Gets or sets the type of the binding.
    /// </summary>
    public SqlFetchParameterBindingType BindingType { get; private set; }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="valueAccessor">Value for <see cref="ValueAccessor"/>.</param>
    /// <param name="typeMapping">Value for <see cref="SqlParameterBinding.TypeMapping"/>.</param>
    /// <param name="bindingType">Value for <see cref="BindingType"/>.</param>
    public SqlFetchParameterBinding(Func<object> valueAccessor, TypeMapping typeMapping, SqlFetchParameterBindingType bindingType)
      : base(typeMapping)
    {
      ValueAccessor = valueAccessor;
      BindingType = bindingType;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>.
    /// </summary>
    /// <param name="valueAccessor"></param>
    /// <param name="typeMapping"></param>
    public SqlFetchParameterBinding(Func<object> valueAccessor, TypeMapping typeMapping)
      : this(valueAccessor, typeMapping, SqlFetchParameterBindingType.Regular)
    {
    }
  }
}
