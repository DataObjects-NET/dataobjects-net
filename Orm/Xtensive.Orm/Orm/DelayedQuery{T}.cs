// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.19

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm.Linq;

namespace Xtensive.Orm.Internals
{
  [Serializable]
  public sealed class DelayedQuery<T> : DelayedQuery, IEnumerable<T>
  {
    public IEnumerator<T> GetEnumerator() => Materialize<T>().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Asynchronously executes delayed query.
    /// </summary>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Value representing query execution result.</returns>
    public ValueTask<QueryResult<T>> ExecuteAsync(CancellationToken token = default) => MaterializeAsync<T>(token);

    // Constructors

    internal DelayedQuery(Session session, TranslatedQuery translatedQuery, ParameterContext parameterContext)
      : base(session, translatedQuery, parameterContext)
    {}
  }
}