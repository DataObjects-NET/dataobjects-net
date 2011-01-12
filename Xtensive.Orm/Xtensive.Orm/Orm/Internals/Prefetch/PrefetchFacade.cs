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
    private readonly Collections.LinkedList<PrefetchFieldNode> nodes;

    public PrefetchFacade<T> RegisterPath<TValue>(Expression<Func<T, TValue>> expression)
    {
      var node = PrefetchNodeParser.Parse(expression);
      return new PrefetchFacade<T>(session, source, nodes.AppendHead(node));
    }

    public IEnumerator<T> GetEnumerator()
    {
      foreach (var item in source)
        yield return item;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public PrefetchFacade(Session session, IEnumerable<Key> keySource)
    {
      this.session = session;
      source = new PrefetchKeyIterator<T>(session, keySource);
      nodes = Collections.LinkedList<PrefetchFieldNode>.Empty;
    }

    public PrefetchFacade(Session session, IEnumerable<T> source)
      : this(session, source, Collections.LinkedList<PrefetchFieldNode>.Empty)
    {}

    private PrefetchFacade(Session session, IEnumerable<T> source, Collections.LinkedList<PrefetchFieldNode> nodes)
    {
      this.session = session;
      this.source = source;
      this.nodes = nodes;
    }
  }
}