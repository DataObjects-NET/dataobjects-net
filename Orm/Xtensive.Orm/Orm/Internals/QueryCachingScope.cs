// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.27

using System;
using Xtensive.Core;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Linq.Expressions.Visitors;

namespace Xtensive.Orm.Internals
{
  internal sealed class QueryCachingScope : IDisposable
  {
    private readonly QueryEndpoint endpoint;
    private TranslatedQuery parameterizedQuery;
    private readonly QueryCachingScope oldScope;

    public Parameter QueryParameter { get; }
    public ExtendedExpressionReplacer QueryParameterReplacer { get; }
    public bool Execute { get; }

    /// <exception cref="NotSupportedException">Second attempt to set this property.</exception>
    public TranslatedQuery ParameterizedQuery {
      get => parameterizedQuery;
      set {
        if (parameterizedQuery != null) {
          throw Exceptions.AlreadyInitialized("ParameterizedQuery");
        }

        parameterizedQuery = value;
      }
    }

    public void Dispose() => endpoint.Provider.QueryCachingScope = oldScope;


    // Constructors

    public QueryCachingScope(QueryEndpoint endpoint, Parameter queryParameter, ExtendedExpressionReplacer queryParameterReplacer, bool execute = true)
    {
      this.endpoint = endpoint;
      QueryParameter = queryParameter;
      QueryParameterReplacer = queryParameterReplacer;
      Execute = execute;
      oldScope = endpoint.Provider.QueryCachingScope;
      endpoint.Provider.QueryCachingScope = this;
    }
  }
}