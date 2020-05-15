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
  internal sealed class QueryCachingScope
  {
    private readonly QueryEndpoint endpoint;
    private TranslatedQuery parameterizedQuery;

    public Parameter QueryParameter { get; }
    public ExtendedExpressionReplacer QueryParameterReplacer { get; }
    public ParameterContext ParameterContext { get; }
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

    public readonly ref struct ActivationScope
    {
      private readonly QueryProvider provider;
      private readonly QueryCachingScope oldScope;

      public void Dispose() => provider.QueryCachingScope = oldScope;

      public ActivationScope(QueryProvider provider)
      {
        this.provider = provider;
        oldScope = provider.QueryCachingScope;
      }
    }

    public ActivationScope Enter()
    {
      var scope = new ActivationScope(endpoint.Provider);
      endpoint.Provider.QueryCachingScope = this;
      return scope;
    }

    // Constructors

    public QueryCachingScope(QueryEndpoint endpoint, Parameter queryParameter,
      ExtendedExpressionReplacer queryParameterReplacer, ParameterContext parameterContext = null, bool execute = true)
    {
      this.endpoint = endpoint;
      ParameterContext = parameterContext;
      QueryParameter = queryParameter;
      QueryParameterReplacer = queryParameterReplacer;
      Execute = execute;
    }
  }
}