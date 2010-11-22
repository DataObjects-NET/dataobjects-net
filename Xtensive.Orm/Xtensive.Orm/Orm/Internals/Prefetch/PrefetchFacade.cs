// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.11.18

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal class PrefetchFacade<T> : IEnumerable<T>
  {
    private readonly Session session;
    private readonly IEnumerable<T> source;
    private readonly IEnumerable<Key> keySource;
    private readonly Collections.LinkedList<PrefetchNode> nodes;


    public PrefetchFacade<T> RegisterPath<TValue>(Expression<Func<T, TValue>> expression)
    {
      var node = PrefetchNodeParser.Parse(expression);
      return new PrefetchFacade<T>(session, source, nodes.AppendHead(node));
    }

    public IEnumerator<T> GetEnumerator()
    {
      if (keySource != null) {
        var sessionHandler = session.Handler;
//        var result = new RootElementsPrefetcher<T>(source, keyExtractor, modelType,
//          fieldDescriptors, sessionHandler);
      }
      throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public PrefetchFacade(Session session, IEnumerable<Key> keySource)
    {
      this.session = session;
      this.keySource = keySource;
      nodes = Collections.LinkedList<PrefetchNode>.Empty;
    }

    public PrefetchFacade(Session session, IEnumerable<T> source)
      : this(session, source, Collections.LinkedList<PrefetchNode>.Empty)
    {}

    private PrefetchFacade(Session session, IEnumerable<T> source, Collections.LinkedList<PrefetchNode> nodes)
    {
      this.session = session;
      this.source = source;
      this.nodes = nodes;
    }
  }
}