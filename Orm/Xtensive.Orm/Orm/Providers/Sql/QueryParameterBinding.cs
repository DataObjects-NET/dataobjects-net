// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.25

using System;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Sql;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// A binding of a parameter for <see cref="QueryRequest"/>.
  /// </summary>
  public class QueryParameterBinding : ParameterBinding
  {
    /// <summary>
    /// Gets the value accessor.
    /// </summary>
    public Func<object> ValueAccessor { get; private set; }

    /// <summary>
    /// Gets the type of the binding.
    /// </summary>
    public QueryParameterBindingType BindingType { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="valueAccessor">Value for <see cref="ValueAccessor"/>.</param>
    /// <param name="typeMapping">Value for <see cref="ParameterBinding.TypeMapping"/>.</param>
    /// <param name="bindingType">Value for <see cref="BindingType"/>.</param>
    public QueryParameterBinding(Func<object> valueAccessor, TypeMapping typeMapping, QueryParameterBindingType bindingType)
      : base(typeMapping)
    {
      ArgumentValidator.EnsureArgumentNotNull(valueAccessor, "valueAccessor");
      switch (bindingType) {
      case QueryParameterBindingType.Regular:
      case QueryParameterBindingType.SmartNull:
        ArgumentValidator.EnsureArgumentNotNull(typeMapping, "typeMapping");
        break;
      }

      ValueAccessor = valueAccessor;
      BindingType = bindingType;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>.
    /// </summary>
    /// <param name="valueAccessor"></param>
    /// <param name="typeMapping"></param>
    public QueryParameterBinding(Func<object> valueAccessor, TypeMapping typeMapping)
      : this(valueAccessor, typeMapping, QueryParameterBindingType.Regular)
    {
    }
  }
}
