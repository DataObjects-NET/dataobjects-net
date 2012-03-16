// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.11.22

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xtensive.Orm.Internals.Prefetch
{
  [Serializable]
  internal class PrefetchProcessor<T> : IEnumerable<T>
  {
    private readonly Session session;
    private readonly IEnumerable<T> source;
    private readonly IEnumerable<Key> keySource;

    public IEnumerator<T> GetEnumerator()
    {
      throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}