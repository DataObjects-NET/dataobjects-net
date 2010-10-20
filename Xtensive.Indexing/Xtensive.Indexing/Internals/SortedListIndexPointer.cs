// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.20

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Indexing.Implementation;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing
{
  [DebuggerDisplay("{Owner} -> {Index}")]
  internal struct SortedListIndexPointer<TKey, TItem> :
    IEquatable<SortedListIndexPointer<TKey, TItem>>
  {
    public SortedListIndex<TKey, TItem> Owner;
    public int Index;

    public TItem Current
    {
      [DebuggerStepThrough]
      get { return Owner[Index]; }
    }
    
    public bool MoveNext(Direction direction)
    {
      Index += (int)direction;
      if (Index < 0) {
        Index -= (int)direction;
        return false;
      }
      else if (Index >= Owner.Count) {
        Index -= (int)direction;
        return false;
      }
      return true;
    }

    #region Equals, GetHashCode methods

    public bool Equals(SortedListIndexPointer<TKey, TItem> obj)
    {
      return obj.Owner==Owner && obj.Index==Index;
    }

    public override bool Equals(object obj)
    {
      if (obj.GetType()!=typeof (SortedListIndexPointer<TKey, TItem>))
        return false;
      return Equals((SortedListIndexPointer<TKey, TItem>) obj);
    }

    public override int GetHashCode()
    {
      unchecked {
        return ((Owner!=null ? Owner.GetHashCode() : 0) * 397) ^ Index;
      }
    }

    #endregion


    // Constructors

    public SortedListIndexPointer(SortedListIndex<TKey, TItem> owner, int index)
    {
      Owner = owner;
      Index = index;
    }
  }
}