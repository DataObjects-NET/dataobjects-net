// Copyright (C) 2012-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.02.27

using System.Collections.Generic;
using System.Linq;
using Xtensive.Sql;

namespace Xtensive.Orm.Services
{
  /// <summary>
  /// Result of LINQ query translation.
  /// </summary>
  public sealed class QueryTranslationResult
  {
    /// <summary>
    /// Gets or sets SQL DOM query.
    /// </summary>
    public ISqlCompileUnit Query { get; set; }

    /// <summary>
    /// Gets or sets parameter bindings for translated query.
    /// </summary>
    public IList<QueryParameterBinding> ParameterBindings { get; set; }

    // Constructors

    internal QueryTranslationResult(ISqlCompileUnit query, IList<QueryParameterBinding> bindings)
    {
      Query = query;
      ParameterBindings = bindings;
    }
  }
}