// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.26

using System.Collections.Generic;
using System.Linq;
using Xtensive.Orm.Providers.Sql;
using Xtensive.Sql.Compiler;

namespace Xtensive.Orm.Services
{
  /// <summary>
  /// Thread-safe immutable session-independent representation of a query.
  /// Note that <see cref="QueryParameterBinding.ValueAccessor"/>
  /// might contain reference to a session thus turning corresponding request
  /// to a session-dependent object.
  /// </summary>
  public sealed class QueryRequest
  {
    /// <summary>
    /// Gets compiled statement.
    /// </summary>
    public SqlCompilationResult CompiledQuery
    {
      get { return RealRequest.GetCompiledStatement(); }
    }

    /// <summary>
    /// Gets all <see cref="QueryParameterBinding"/> associated with this request.
    /// </summary>
    public IEnumerable<QueryParameterBinding> ParameterBindings
    {
      get { return RealRequest.ParameterBindings.Select(b => new QueryParameterBinding(b)); }
    }

    internal UserQueryRequest RealRequest { get; private set; }

    // Constructors

    internal QueryRequest(UserQueryRequest realRequest)
    {
      RealRequest = realRequest;
    }
  }
}