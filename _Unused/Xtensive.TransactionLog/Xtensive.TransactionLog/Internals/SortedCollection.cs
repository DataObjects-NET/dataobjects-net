// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.09.28

using System;
using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.TransactionLog
{
  /// <summary>
  /// Generic collection, that stores values in order of <see cref="IComparer{T}"/>.
  /// </summary>
  /// <typeparam name="T">Type parameter of an element of the collection.</typeparam>
  [Serializable]
  internal sealed class SortedCollection<T> 
  {
    private readonly IComparer<T> comparer;
    private readonly List<T> innerList;

    public void Add(T item)
    {
      int index = innerList.BinarySearch(item, comparer);
      if (index < 0) {
        innerList.Insert(~index, item);
      }
      else {
        innerList.Insert(index, item);
      }
    }

    public bool Contains(T item)
    {
      int index = innerList.BinarySearch(item, comparer);
      return index >= 0;
    }

    public bool Remove(T item)
    {
      int index = innerList.BinarySearch(item, comparer);
      if (index < 0)
        return false;
      innerList.RemoveAt(index);
      return true;
    }

    public int Count
    {
      get { return innerList.Count; }
    }

    public T this[int index]
    {
      get { return innerList[index]; }
    }

    public int BinarySearch(T key)
    {
      return innerList.BinarySearch(key, comparer);
    }

    public void RemoveRange(int index, int count)
    {
      innerList.RemoveRange(index, count);
    }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate" copy="true"/>
    /// </summary>
    public SortedCollection() : this(Comparer<T>.Default)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate" copy="true"/>
    /// </summary>
    /// <param name="comparer">Comparer for elements of the collection.</param>
    public SortedCollection(IComparer<T> comparer)
    {
      this.comparer = comparer;
      innerList = new List<T>();
    }
  }
}