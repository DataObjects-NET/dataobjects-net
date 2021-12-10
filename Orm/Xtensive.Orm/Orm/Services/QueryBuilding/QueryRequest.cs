// Copyright (C) 2012-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.02.26

using System.Collections.Generic;
using System.Linq;
using Xtensive.Orm.Providers;
using Xtensive.Sql.Compiler;

namespace Xtensive.Orm.Services
{
  /// <summary>
  /// Thread-safe immutable session-independent representation of a query.
  /// Note that <see cref="QueryParameterBinding.ValueAccessor"/>
  /// might contain reference to a session thus turning corresponding request
  /// to a session-dependent object.
  /// </summary>
  public readonly struct QueryRequest
  {
    /// <summary>
    /// Gets compiled statement.
    /// </summary>
    public SqlCompilationResult CompiledQuery => RealRequest.GetCompiledStatement();

    /// <summary>
    /// Gets all <see cref="QueryParameterBinding"/> associated with this request.
    /// </summary>
    public IEnumerable<QueryParameterBinding> ParameterBindings =>
      RealRequest.ParameterBindings.Select(b => new QueryParameterBinding(b));

    internal UserQueryRequest RealRequest { get; }

    // Constructors

    internal QueryRequest(UserQueryRequest realRequest)
    {
      RealRequest = realRequest;
    }
  }
}
