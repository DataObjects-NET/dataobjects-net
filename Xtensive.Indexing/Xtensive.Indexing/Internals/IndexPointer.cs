// Copyright (C) 2008 Xtensive LLC.
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
  [DebuggerDisplay("{Page} -> {Index}")]
  internal struct IndexPointer<TKey, TItem> :
    IEquatable<IndexPointer<TKey, TItem>>
  {
    public LeafPage<TKey, TItem> Page;
    public int Index;

    public TItem Current
    {
      [DebuggerStepThrough]
      get { return Page[Index]; }
    }
    
    public bool MoveNext(Direction direction)
    {
      Index += (int)direction;
      if (Index < 0) {
        if (Page.LeftPageRef!=null) {
          Page = Page.LeftPage;
          Index = Page.CurrentSize - 1;
          return true;
        }
        else {
          Index -= (int)direction;
          return false;
        }
      }
      else if (Index >= Page.CurrentSize) {
        if (Page.RightPageRef!=null) {
          Page = Page.RightPage;
          Index = 0;
          return true;
        }
        else {
          Index -= (int)direction;
          return false;
        }
      }
      return true;
    }

    #region Equals, GetHashCode methods

    public bool Equals(IndexPointer<TKey, TItem> obj)
    {
      return obj.Page==Page && obj.Index==Index;
    }

    public override bool Equals(object obj)
    {
      if (obj.GetType()!=typeof (IndexPointer<TKey, TItem>))
        return false;
      return Equals((IndexPointer<TKey, TItem>) obj);
    }

    public override int GetHashCode()
    {
      unchecked {
        return ((Page!=null ? Page.GetHashCode() : 0) * 397) ^ Index;
      }
    }

    #endregion


    // Constructors

    public IndexPointer(LeafPage<TKey, TItem> page, int index)
    {
      Page = page;
      Index = index;
    }
  }
}