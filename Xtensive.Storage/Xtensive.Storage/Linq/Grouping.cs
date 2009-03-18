// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.03.17

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace Xtensive.Storage.Linq
{
  [Serializable]
  internal class Grouping<TKey, TElement> : 
    IGrouping<TKey, TElement>
  {
    private readonly TKey key;
    private readonly IEnumerable<TElement> enumerable;

    public TKey Key
    {
      get { return key; }
    }

    public IEnumerator<TElement> GetEnumerator()
    {
      return enumerable.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    internal Grouping(TKey key, IEnumerable<TElement> enumerable)
    {
      this.enumerable = enumerable;
      this.key = key;
    }
  }
}