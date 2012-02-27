// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.09.09

using System;
using Xtensive.Core;
using Xtensive.Disposing;

namespace Xtensive.Orm
{
  partial class Session
  {
    /// <summary>
    /// Single access point allowing to run LINQ queries,
    /// create future (delayed) and compiled queries,
    /// and finally, resolve <see cref="Key"/>s to <see cref="Entity">entities</see>.
    /// </summary>
    public QueryEndpoint Query { get; private set; }

    /// <summary>
    /// <see cref="QueryEndpoint"/> that always uses default <see cref="IQueryRootBuilder"/>
    /// ignoring any calls to <see cref="OverrideQueryRoot"/>.
    /// </summary>
    public QueryEndpoint SystemQuery { get; private set; }

    /// <summary>
    /// Overrides current query root builder (i.e. builder of <see cref="QueryEndpoint.All{T}"/> expression).
    /// After this method is called <see cref="Query"/> is changed to a new <see cref="QueryEndpoint"/>
    /// that utilizes specified <paramref name="queryRootBuilder"/>.
    /// </summary>
    /// <param name="queryRootBuilder"><see cref="IQueryRootBuilder"/> to use.</param>
    /// <returns><see cref="IDisposable"/> implementor
    /// that reverts <see cref="IQueryRootBuilder"/> to original
    /// once <see cref="IDisposable.Dispose"/> is called.</returns>
    public IDisposable OverrideQueryRoot(IQueryRootBuilder queryRootBuilder)
    {
      ArgumentValidator.EnsureArgumentNotNull(queryRootBuilder, "queryRootBuilder");
      var oldQuery = Query;
      var newQuery = new QueryEndpoint(oldQuery, queryRootBuilder);
      Query = newQuery;
      return new Disposable(_ => Query = oldQuery);
    }
  }
}
