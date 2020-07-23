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
  /// <summary>
  /// Represents a delayed sequence query (query where result can be enumerated after an execution).
  /// </summary>
  /// <typeparam name="TElement">The type of the element in a resulting sequence.</typeparam>
  [Serializable]
  public sealed class DelayedQuery<TElement> : DelayedQuery, IEnumerable<TElement>
  {
    /// <inheritdoc/>
    public IEnumerator<TElement> GetEnumerator() => Materialize<TElement>().GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Asynchronously executes delayed query.
    /// </summary>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Value representing query execution result.</returns>
    public ValueTask<QueryResult<TElement>> ExecuteAsync(CancellationToken token = default) =>
      MaterializeAsync<TElement>(token);

    // Constructors

    internal DelayedQuery(Session session, TranslatedQuery translatedQuery, ParameterContext parameterContext)
      : base(session, translatedQuery, parameterContext)
    {}
  }
}