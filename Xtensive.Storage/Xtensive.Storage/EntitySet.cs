// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.01

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  public class EntitySet<T>: SessionBound, 
    ISet<T>,
    IFieldHandler
    where T: Entity
  {
    private HashSet<Key> set = new HashSet<Key>();

    /// <inheritdoc/>
    public Persistent Owner { get; internal set; }

    /// <inheritdoc/>
    public FieldInfo Field { get; internal set; }

    /// <inheritdoc/>
    void ICollection<T>.Add(T item)
    {
      Add(item);
    }

    /// <inheritdoc/>
    public void Clear()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public bool Contains(T item)
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex)
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public bool Remove(T item)
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    int ICollection<T>.Count
    {
      get { throw new NotImplementedException(); }
    }

    /// <inheritdoc/>
    long ICountable.Count
    {
      get { throw new NotImplementedException(); }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable<T>) this).GetEnumerator();
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public bool Add(T item)
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public int RemoveWhere(Predicate<T> match)
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public T this[T item]
    {
      get { throw new NotImplementedException(); }
    }

    /// <inheritdoc/>
    public int Count
    {
      get { throw new NotImplementedException(); }
    }

    /// <inheritdoc/>
    public bool IsReadOnly
    {
      get { throw new NotImplementedException(); }
    }

    /// <inheritdoc/>
    public IEqualityComparer<T> Comparer
    {
      get { throw new NotImplementedException(); }
    }
  }
}