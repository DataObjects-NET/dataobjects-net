// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.14

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xtensive.Collections
{
  /// <summary>
  /// Read-only wrapper over the <see cref="HashSet{T}"/> class instance.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("Count = {Count}")]
  internal class ReadOnlyHashSet<T>: System.Collections.Generic.ISet<T>, IReadOnlySet<T>
  {
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private readonly HashSet<T> innerSet;

    /// <summary>
    /// Gets the number of elements that are contained in a set.
    /// </summary>
    public int Count => innerSet.Count;

    /// <inheritdoc/>
    bool ICollection<T>.IsReadOnly => true;

    /// <summary>
    /// Determines whether a <see href="ReadOnlyHashSet{T}"/> object contains the specified element.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the <see href="ReadOnlyHashSet{T}"/> object contains
    /// the specified <see paramref="item"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(T item) => innerSet.Contains(item);

    /// <summary>
    /// Returns an enumerator that iterates through a <see href="ReadOnlyHashSet{T}"/> object.
    /// </summary>
    /// <returns>A <see href="HashSet{T}.Enumerator"/> object for the <see href="ReadOnlyHashSet{T}"/> object.</returns>
    public HashSet<T>.Enumerator GetEnumerator() => innerSet.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Determines whether a <see href="ReadOnlyHashSet{T}"/> object is a proper subset of the specified collection.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the <see href="ReadOnlyHashSet{T}"/> object is a proper subset of 
    /// the specified <see paramref="other"/> collection; otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsProperSubsetOf(IEnumerable<T> other) => innerSet.IsProperSubsetOf(other);

    /// <summary>
    /// Determines whether a <see href="ReadOnlyHashSet{T}"/> object is a proper superset of the specified collection.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the <see href="ReadOnlyHashSet{T}"/> object is a proper superset of 
    /// the specified <see paramref="other"/> collection; otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsProperSupersetOf(IEnumerable<T> other) => innerSet.IsProperSupersetOf(other);

    /// <summary>
    /// Determines whether a <see href="ReadOnlyHashSet{T}"/> object is a subset of the specified collection.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the <see href="ReadOnlyHashSet{T}"/> object is a subset of 
    /// the specified <see paramref="other"/> collection; otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsSubsetOf(IEnumerable<T> other) => innerSet.IsSubsetOf(other);

    /// <summary>
    /// Determines whether a <see href="ReadOnlyHashSet{T}"/> object is a superset of the specified collection.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the <see href="ReadOnlyHashSet{T}"/> object is a superset of 
    /// the specified <see paramref="other"/> collection; otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsSupersetOf(IEnumerable<T> other) => innerSet.IsSupersetOf(other);
    
    /// <summary>
    /// Determines whether the current <see href="ReadOnlyHashSet{T}"/> object and a specified collection share common elements.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the <see href="ReadOnlyHashSet{T}"/> object object and <see paramref="other"/> collection
    /// share at least one common element; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Overlaps(IEnumerable<T> other) => innerSet.Overlaps(other);

    /// <summary>
    /// Determines whether a <see href="ReadOnlyHashSet{T}"/> object and the specified <see paramref="other"/> collection
    /// contain the same elements.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the <see href="ReadOnlyHashSet{T}"/> object object is equal to the <see paramref="other"/> collection;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool SetEquals(IEnumerable<T> other) => innerSet.SetEquals(other);

    /// <summary>
    /// Copies the elements of a <see cref="ReadOnlyHashSet{T}"/> object to an <see paramref="array"/>,
    /// starting at the specified <see paramref="arrayIndex"/>.
    /// </summary>
    public void CopyTo(T[] array, int arrayIndex) => innerSet.CopyTo(array, arrayIndex);

    #region Not Implemented Members

    bool System.Collections.Generic.ISet<T>.Add(T item) => throw new NotImplementedException();
    void System.Collections.Generic.ISet<T>.ExceptWith(IEnumerable<T> other) => throw new NotImplementedException();
    void System.Collections.Generic.ISet<T>.IntersectWith(IEnumerable<T> other) => throw new NotImplementedException();
    void System.Collections.Generic.ISet<T>.SymmetricExceptWith(IEnumerable<T> other) => throw new NotImplementedException();
    void System.Collections.Generic.ISet<T>.UnionWith(IEnumerable<T> other) => throw new NotImplementedException();
    void ICollection<T>.Add(T item) => throw new NotImplementedException();
    void ICollection<T>.Clear() => throw new NotImplementedException();
    bool ICollection<T>.Remove(T item) => throw new NotImplementedException();

    #endregion

    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="set">The set to wrap.</param>
    public ReadOnlyHashSet(HashSet<T> set)
    {
      innerSet = set;
    }
  }
}