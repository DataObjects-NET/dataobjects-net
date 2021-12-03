// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.11.12

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
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
    public IReadOnlyList<TypeMapping> RowTypeMapping { get; private set; }

    public QueryRowFilterParameterBinding(IEnumerable<TypeMapping> rowTypeMapping, Func<ParameterContext, object> valueAccessor)
      : base(null, valueAccessor, QueryParameterBindingType.RowFilter)
    {
      RowTypeMapping = rowTypeMapping.ToList().AsReadOnly();
    }
  }
}