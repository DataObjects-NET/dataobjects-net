// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Collections;
using Xtensive.Orm.Internals.Prefetch;

namespace Xtensive.Orm
{
  /// <summary>
  /// Represents query configured to perform additional fetch operations for the entities
  /// reachable by following references from <typeparamref name="TElement"/>s of the
  /// initial query result.
  /// </summary>
  /// <typeparam name="TElement">The type of the queried elements.</typeparam>
  public readonly struct PrefetchQuery<TElement> : IEnumerable<TElement>
  {
    private readonly Session session;
    private readonly IEnumerable<TElement> source;
    private readonly SinglyLinkedList<KeyExtractorNode<TElement>> nodes;

    internal PrefetchQuery<TElement> RegisterPath<TValue>(Expression<Func<TElement, TValue>> expression)
    {
      var node = NodeBuilder.Build(session.Domain.Model, expression);
      return node==null ? this : new PrefetchQuery<TElement>(session, source, nodes.Add(node));
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public IEnumerator<TElement> GetEnumerator() =>
      new PrefetchQueryEnumerable<TElement>(session, source, nodes).GetEnumerator();

    /// <summary>
    /// Transforms <see cref="PrefetchQuery{TElement}"/> to an <see cref="IAsyncEnumerable{T}"/> sequence.
    /// </summary>
    public IAsyncEnumerable<TElement> AsAsyncEnumerable() =>
      new PrefetchQueryAsyncEnumerable<TElement>(session, source, nodes);

    private class ExecuteAsyncResult : IEnumerable<TElement>
    {
      private readonly List<TElement> items;
      // We need to hold StrongReferenceContainer to prevent loaded entities from being collected
      private readonly StrongReferenceContainer referenceContainer;

      IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

      public IEnumerator<TElement> GetEnumerator() => items.GetEnumerator();

      public ExecuteAsyncResult(List<TElement> items, StrongReferenceContainer referenceContainer)
      {
        this.items = items;
        this.referenceContainer = referenceContainer;
      }
    }

    /// <summary>
    /// Asynchronously executes given <see cref="PrefetchQuery{TElement}"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method internally puts all elements of the resulting sequence to a list
    /// and then it wraps mentioned list as a <see cref="QueryResult{TItem}"/>.
    /// As a consequence it is more efficient to use asynchronous enumeration over result of
    /// <see cref="AsAsyncEnumerable"/> method call because it can perform lazily
    /// not putting everything into intermediate list.
    /// </para>
    /// <para> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.
    /// </para>
    /// </remarks>
    /// <param name="token">The token to cancel operation if needed.</param>
    /// <returns>A task performing operation.</returns>
    public async Task<QueryResult<TElement>> ExecuteAsync(CancellationToken token = default)
    {
      var list = new List<TElement>();
      var asyncEnumerable = new PrefetchQueryAsyncEnumerable<TElement>(session, source, nodes);
      await foreach (var element in asyncEnumerable.WithCancellation(token).ConfigureAwaitFalse()) {
        list.Add(element);
      }

      return new QueryResult<TElement>(new ExecuteAsyncResult(list, asyncEnumerable.StrongReferenceContainer));
    }

    internal PrefetchQuery(Session session, IEnumerable<Key> keySource)
      : this(session, new PrefetchKeyIterator<TElement>(session, keySource), SinglyLinkedList<KeyExtractorNode<TElement>>.Empty)
    { }

    internal PrefetchQuery(Session session, IEnumerable<TElement> source)
      : this(session, source, SinglyLinkedList<KeyExtractorNode<TElement>>.Empty)
    { }

    private PrefetchQuery(Session session, IEnumerable<TElement> source, SinglyLinkedList<KeyExtractorNode<TElement>> nodes)
    {
      this.session = session;
      this.source = source;
      this.nodes = nodes;
    }
  }
}