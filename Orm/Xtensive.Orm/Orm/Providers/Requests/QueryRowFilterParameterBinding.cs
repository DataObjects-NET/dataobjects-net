// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.12

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Sql;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// A special version of <see cref="QueryParameterBinding"/> used for complex filters.
  /// </summary>
  public class QueryRowFilterParameterBinding : QueryParameterBinding
  {
    /// <summary>
    /// Gets the complex type mapping.
    /// </summary>
    public ReadOnlyList<TypeMapping> RowTypeMapping { get; private set; }

    public QueryRowFilterParameterBinding(IEnumerable<TypeMapping> rowTypeMapping, Func<object> valueAccessor)
      : base(null, valueAccessor, QueryParameterBindingType.RowFilter)
    {
      RowTypeMapping = new ReadOnlyList<TypeMapping>(rowTypeMapping.ToList());
    }
  }
}