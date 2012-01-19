// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.06.13

using System;
using Xtensive.Collections;
using System.Collections;
using Xtensive.Core;

namespace Xtensive.Collections
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