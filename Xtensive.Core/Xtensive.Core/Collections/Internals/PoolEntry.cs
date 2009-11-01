// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.06.13

using System;
using Xtensive.Core.Collections;
using System.Collections;

namespace Xtensive.Core.Collections
{
  internal class PoolEntry<T>
  {
    public readonly ISet<T> BusyObjects = new SetSlim<T>();
    public readonly IPriorityQueue<T, DateTime> FreeObjects = new PriorityQueue<T, DateTime>(Direction.Positive);

    public long Count
    {
      get { return ((ICollection)BusyObjects).Count + FreeObjects.Count; }
    }
  }
}