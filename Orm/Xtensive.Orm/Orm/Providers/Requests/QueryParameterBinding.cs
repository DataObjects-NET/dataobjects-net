// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2008.09.25

using System;
using Xtensive.Core;
using Xtensive.Sql;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// A binding of a parameter for <see cref="QueryRequest"/>.
  /// </summary>
  public class QueryParameterBinding : ParameterBinding
  {
    public Func<ParameterContext, object> ValueAccessor { get; private set; }

    public QueryParameterBindingType BindingType { get; private set; }

    // Constructors

    private QueryParameterBinding(TypeMapping typeMapping, Func<ParameterContext, object> valueAccessor,
      ParameterTransmissionType transmissionType, QueryParameterBindingType bindingType)
      : base(typeMapping, transmissionType)
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

    public QueryParameterBinding(TypeMapping typeMapping, Func<ParameterContext, object> valueAccessor, ParameterTransmissionType transmissionType)
      : this(typeMapping, valueAccessor, transmissionType, QueryParameterBindingType.Regular)
    {
    }

    public QueryParameterBinding(TypeMapping typeMapping, Func<ParameterContext, object> valueAccessor, QueryParameterBindingType bindingType)
      : this(typeMapping, valueAccessor, ParameterTransmissionType.Regular, bindingType)
    {
    }

    public QueryParameterBinding(TypeMapping typeMapping, Func<ParameterContext, object> valueAccessor)
      : this(typeMapping, valueAccessor, ParameterTransmissionType.Regular, QueryParameterBindingType.Regular)
    {
    }
  }
}
