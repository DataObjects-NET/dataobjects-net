using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
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

    IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

    IEnumerator<TItem> IEnumerable<TItem>.GetEnumerator() => throw new NotImplementedException();

    public readonly struct PrefetchQueryEnumerator : IEnumerator<TItem>
    {
      public bool MoveNext() => throw new NotImplementedException();

      public void Reset() => throw new NotImplementedException();

      public TItem Current { get; }

      object IEnumerator.Current => Current;

      public void Dispose() => throw new NotImplementedException();
    }

    public PrefetchQueryEnumerator GetEnumerator() => new PrefetchQueryEnumerator();

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