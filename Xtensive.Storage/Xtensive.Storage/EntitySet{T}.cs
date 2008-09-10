// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.01

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  public abstract class EntitySet<T>: EntitySet, 
    ISet<T>
    where T: Entity
  {
    internal static int MaximumCacheSize = 10000;

    private readonly AdvancedComparer<T> comparer = AdvancedComparer<T>.Default;

    /// <inheritdoc/>
    public abstract long Count { get; }

    /// <inheritdoc/>
    public abstract T this[T item] { get; }

    /// <inheritdoc/>
    public abstract bool Contains(T item);

    #region Modification methods: Add, Remove, RemoveWhere, Clear, CopyTo

    /// <inheritdoc/>
    public abstract bool Add(T item);

    /// <inheritdoc/>
    public abstract bool Remove(T item);

    /// <inheritdoc/>
    public abstract int RemoveWhere(Predicate<T> match);

    /// <inheritdoc/>
    public abstract void Clear();

    /// <inheritdoc/>
    public abstract void CopyTo(T[] array, int arrayIndex);

    #endregion

    #region ISet<T> methods

    int ISet<T>.Count
    {
      get { return checked ((int) Count); }
    }

    #endregion

    #region ICollection<T> methods

    /// <inheritdoc/>
    int ICollection<T>.Count
    {
      get { return checked ((int) Count); }
    }

    /// <inheritdoc/>
    void ICollection<T>.Add(T item)
    {
      Add(item);
    }

    #endregion

    #region IEnumerable<...> methods

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
      throw new NotImplementedException();
    }

    #endregion

    /// <inheritdoc/>
    public IEqualityComparer<T> Comparer
    {
      get { return comparer.EqualityComparerImplementation; }
    }

    /// <inheritdoc/>
    public bool IsReadOnly
    {
      get { return false; }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="owner">Persistent this entity set belongs to.</param>
    /// <param name="field">Field corresponds to this entity set.</param>
    protected EntitySet(Persistent owner, FieldInfo field)
      :base(owner, field)
    {
    }
  }
}