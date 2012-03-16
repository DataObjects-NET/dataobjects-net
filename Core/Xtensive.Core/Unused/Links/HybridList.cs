// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using System.Collections.Generic;

namespace Xtensive.Core.Links
{
  internal struct HybridList<T>
  {
    private byte count;
    private List<T> innerList;
    public T Item0;
    public T Item1;
    public T Item2;
    public T Item3;

    public int Count
    {
      get { return innerList==null ? count : innerList.Count; }
    }

    public T this[int index]
    {
      get
      {
        if (innerList!=null)
          return innerList[index];
        if (index < 2)
          return index==1 ? Item1 : Item0;
        else
          return index==2 ? Item2 : Item3;
      }
    }

    public void Add(ref T item)
    {
      if (count==4 && innerList==null) {
        innerList = new List<T>(5);
        innerList.Add(Item0);
        innerList.Add(Item1);
        innerList.Add(Item2);
        innerList.Add(Item3);
      }

      if (innerList!=null) {
        innerList.Add(item);
        return;
      }

      if (count < 2)
        if (count==1)
          Item1 = item;
        else
          Item0 = item;
      else if (count==2)
        Item2 = item;
      else
        Item3 = item;
      count++;
    }

    public void Clear()
    {
      if (innerList!=null)
        innerList.Clear();
      else
        count = 0;
      Item0 = default(T);
      Item1 = default(T);
      Item2 = default(T);
      Item3 = default(T);
    }
  }
}