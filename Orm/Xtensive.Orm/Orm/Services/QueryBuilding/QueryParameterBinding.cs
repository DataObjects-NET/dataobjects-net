// Copyright (C) 2012-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.02.26

using System;
using Xtensive.Core;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Services
{
  /// <summary>
  /// Binding of a query parameter.
  /// </summary>
  public readonly struct QueryParameterBinding
  {
    /// <summary>
    /// Gets type of the parameter.
    /// Internally created <see cref="QueryParameterBinding"/>s
    /// may have this property set to <see langword="null"/>.
    /// Any user-created <see cref="QueryParameterBinding"/>
    /// always has this property set to non <see langword="null"/> value.
    /// </summary>
    public Type ValueType => RealBinding.TypeMapping?.Type;

    /// <summary>
    /// Gets accessor of the parameter.
    /// This delegate returns type that is assignable
    /// to <see cref="ValueType"/>
    /// unless <see cref="ValueType"/> is null.
    /// </summary>
    public Func<ParameterContext, object> ValueAccessor => RealBinding.ValueAccessor;

    /// <summary>
    /// Gets <see cref="SqlExpression"/> that allows
    /// to access parameter in SQL DOM query.
    /// </summary>
    public SqlExpression ParameterReference => RealBinding.ParameterReference;

    internal Providers.QueryParameterBinding RealBinding { get; }

    // Constructors

    internal QueryParameterBinding(Providers.QueryParameterBinding realBinding)
    {
      RealBinding = realBinding;
    }
  }
}