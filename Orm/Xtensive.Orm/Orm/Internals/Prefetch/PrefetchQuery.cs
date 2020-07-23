using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Collections;

namespace Xtensive.Orm.Internals.Prefetch
{
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

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<TElement> GetEnumerator() =>
      new PrefetchQueryEnumerable<TElement>(session, source, nodes).GetEnumerator();

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

    public async Task<QueryResult<TElement>> ExecuteAsync(CancellationToken token = default)
    {
      var list = new List<TElement>();
      var asyncEnumerable = new PrefetchQueryAsyncEnumerable<TElement>(session, source, nodes);
      await foreach (var element in asyncEnumerable.WithCancellation(token).ConfigureAwait(false)) {
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