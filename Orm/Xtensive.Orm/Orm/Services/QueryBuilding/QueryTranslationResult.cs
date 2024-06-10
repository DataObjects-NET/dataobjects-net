// Copyright (C) 2012-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.02.27

using System.Collections.Generic;
using System.Linq;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Services
{
  /// <summary>
  /// Result of LINQ query translation.
  /// </summary>
  public readonly struct QueryTranslationResult
  {
    /// <summary>
    /// Gets <see cref="SqlSelect"/> query.
    /// </summary>
    public SqlSelect Query { get; }

    /// <summary>
    /// Gets parameter bindings for translated query.
    /// </summary>
    public IReadOnlyList<QueryParameterBinding> ParameterBindings { get; }

    // Constructors

    internal QueryTranslationResult(SqlSelect query, IReadOnlyList<QueryParameterBinding> bindings)
    {
      Query = query;
      ParameterBindings = bindings;
    }
  }
}
