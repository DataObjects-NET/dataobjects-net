// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexander Nikolaev
// Created:    2009.08.19

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Linq;

namespace Xtensive.Orm
{
  /// <summary>
  /// Represents a delayed sequence query (query where result can be enumerated after an execution).
  /// </summary>
  /// <typeparam name="TElement">The type of the element in a resulting sequence.</typeparam>
  [Serializable]
  public sealed class DelayedQuery<TElement> : DelayedQuery, IEnumerable<TElement>, IAsyncEnumerable<TElement>
  {
    /// <inheritdoc/>
    public IEnumerator<TElement> GetEnumerator() => Materialize<TElement>().GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    async IAsyncEnumerator<TElement> IAsyncEnumerable<TElement>.GetAsyncEnumerator(CancellationToken token)
    {
      var elements = await ExecuteAsync(token).ConfigureAwaitFalse();
      foreach (var element in elements) {
        yield return element;
      }
    }

    /// <summary>
    /// Asynchronously executes delayed query.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Value representing query execution result.</returns>
    public ValueTask<QueryResult<TElement>> ExecuteAsync(CancellationToken token = default) =>
      MaterializeAsync<TElement>(token);

    // Constructors

    internal DelayedQuery(Session session, TranslatedQuery translatedQuery, ParameterContext parameterContext)
      : base(session, translatedQuery, parameterContext)
    { }

  }
}