using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Collections;

namespace Xtensive.Orm.Internals.Prefetch
{
  public readonly struct PrefetchQuery<TItem> : IEnumerable<TItem>
  {
    private readonly Session session;
    private readonly IEnumerable<TItem> source;
    private readonly SinglyLinkedList<KeyExtractorNode<TItem>> nodes;

    internal PrefetchQuery<TItem> RegisterPath<TValue>(Expression<Func<TItem, TValue>> expression)
    {
      var node = NodeBuilder.Build(session.Domain.Model, expression);
      return node==null ? this : new PrefetchQuery<TItem>(session, source, nodes.Add(node));
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<TItem> GetEnumerator() =>
      new PrefetchQueryEnumerable<TItem>(session, source, nodes).GetEnumerator();

    public IAsyncEnumerable<TItem> AsAsyncEnumerable() =>
      new PrefetchQueryAsyncEnumerable<TItem>(session, source, nodes);

    public async Task<IEnumerable<TItem>> ExecuteAsync(CancellationToken token = default)
    {
      var list = new List<TItem>();
      await foreach (var element in AsAsyncEnumerable().WithCancellation(token)) {
        list.Add(element);
      }

      return list;
    }

    internal PrefetchQuery(Session session, IEnumerable<Key> keySource)
      : this(session, new PrefetchKeyIterator<TItem>(session, keySource), SinglyLinkedList<KeyExtractorNode<TItem>>.Empty)
    { }

    internal PrefetchQuery(Session session, IEnumerable<TItem> source)
      : this(session, source, SinglyLinkedList<KeyExtractorNode<TItem>>.Empty)
    { }

    private PrefetchQuery(Session session, IEnumerable<TItem> source, SinglyLinkedList<KeyExtractorNode<TItem>> nodes)
    {
      this.session = session;
      this.source = source;
      this.nodes = nodes;
    }
  }
}