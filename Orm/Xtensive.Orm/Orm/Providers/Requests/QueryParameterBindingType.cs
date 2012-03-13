// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.05

using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Possible types of <see cref="QueryParameterBinding"/>.
  /// </summary>
  public enum QueryParameterBindingType
  {
    /// <summary>
    /// Indicates no special handling of parameter values.
    /// </summary>
    Regular,
    /// <summary>
    /// Indicates that special handling of null values is performed.
    /// If <see cref="QueryParameterBinding.ValueAccessor"/> returns <see langword="null"/>
    /// generated query with contain "something is null" check instead of "something = @p".
    /// </summary>
    SmartNull,
    /// <summary>
    /// Indicates that <see cref="bool"/> parameters is automatically propagated to constants
    /// according to a value returned by <see cref="QueryParameterBinding.ValueAccessor"/>.
    /// <see cref="ParameterBinding.TypeMapping"/> is ignored in this case.
    /// </summary>
    BooleanConstant,
    /// <summary>
    /// Indicates that parameter is a argument for paging operators
    /// and should be inlined in query as constant value.
    /// <see cref="ParameterBinding.TypeMapping"/> is ignored in this case.
    /// </summary>
    LimitOffset,
    /// <summary>
    /// Indicates that parameter is row filter argument (i.e. a number of parameter vectors).
    /// <see cref="QueryParameterBinding.ValueAccessor"/> returns a collection of <see cref="Tuple"/>s.
    /// <see cref="ParameterBinding.TypeMapping"/> is ignored in this case.
    /// </summary>
    RowFilter,
  }
}