// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.06.14

using System.Collections.Generic;
using System;
using Xtensive.Core;

namespace Xtensive.Collections
{
  internal class PriorityQueueItemComparer<T, P> : IComparer<Pair<T, P>> where P:IComparable<P>
  {
    private Direction direction;

    #region IComparer<Pair<T,TPriority>> Members

    ///<summary>
    ///Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
    ///</summary>
    ///
    ///<returns>
    ///Value Condition Less than zerox is less than y.Zerox equals y.Greater than zerox is greater than y.
    ///</returns>
    ///
    ///<param name="y">The second object to compare.</param>
    ///<param name="x">The first object to compare.</param>
    public int Compare(Pair<T, P> x, Pair<T, P> y)
    {
      return y.Second.CompareTo(x.Second) * (int)direction;
    }

    #endregion


    // Constructors
    
    public PriorityQueueItemComparer(Direction direction)
    {
      this.direction = direction;
    }
  }
}