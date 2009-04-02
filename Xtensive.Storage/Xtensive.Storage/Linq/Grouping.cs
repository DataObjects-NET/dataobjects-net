// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.03.17

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Xtensive.Storage.Linq
{
  [Serializable]
  internal class Grouping<TKey, TElement> : 
    IGrouping<TKey, TElement>
  {
    private readonly TKey key;
    private readonly IQueryable<TElement> queryable;

    public TKey Key
    {
      get { return key; }
    }

    public IQueryable<TElement> Queryable
    {
      get { return queryable; }
    }

    public IEnumerator<TElement> GetEnumerator()
    {
      return queryable.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public Grouping(TKey key, IQueryable<TElement> queryable, object resultExpression)
    {
      this.queryable = queryable;
      this.key = key;
    }
  }
}