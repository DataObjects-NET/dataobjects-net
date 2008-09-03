// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.03

using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.Sql
{
  internal sealed class SqlLoadProvider : SqlProvider,
    IHasNamedResult
  {
    private readonly LoadProvider concreteOrigin;

    /// <inheritdoc/>
    public TemporaryDataScope Scope
    {
      get { return concreteOrigin.Scope; }
    }

    /// <inheritdoc/>
    public string Name
    {
      get { return concreteOrigin.Name; }
    }

    public SqlLoadProvider(CompilableProvider origin, SqlQueryRequest request, HandlerAccessor handlers, params ExecutableProvider[] sources)
      : base(origin, request, handlers, sources)
    {
      concreteOrigin = origin as LoadProvider;
      AddService<IHasNamedResult>();
    }
  }
}