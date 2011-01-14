// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.11.18

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Collections;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal class PrefetchFacade<T> : IEnumerable<T>
  {
    private readonly Session session;
    private readonly IEnumerable<T> source;
    private readonly Collections.LinkedList<KeyExtractorNode<T>> nodes;

    public PrefetchFacade<T> RegisterPath<TValue>(Expression<Func<T, TValue>> expression)
    {
      var node = NodeParser.Parse(session.Domain.Model, expression);
      return new PrefetchFacade<T>(session, source, nodes.AppendHead(node));
    }

    public IEnumerator<T> GetEnumerator()
    {
      var aggregatedNodes = AggregateNodes();
      foreach (var item in source)
        yield return item;
    }

    private IList<KeyExtractorNode<T>> AggregateNodes()
    {

      throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public PrefetchFacade(Session session, IEnumerable<Key> keySource)
    {
      this.session = session;
      source = new PrefetchKeyIterator<T>(session, keySource);
      nodes = Collections.LinkedList<KeyExtractorNode<T>>.Empty;
    }

    public PrefetchFacade(Session session, IEnumerable<T> source)
      : this(session, source, Collections.LinkedList<KeyExtractorNode<T>>.Empty)
    {}

    private PrefetchFacade(Session session, IEnumerable<T> source, Collections.LinkedList<KeyExtractorNode<T>> nodes)
    {
      this.session = session;
      this.source = source;
      this.nodes = nodes;
    }
  }
}