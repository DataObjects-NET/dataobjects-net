// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections;

namespace Xtensive.Collections
{
  /// <summary>
  /// Set of items.
  /// </summary>
  public interface ISet: ICollection
  {
    /// <summary>
    /// Gets the <see cref="System.Object"/> with the specified item.
    /// </summary>
    object this[object item] { get; }

    /// <summary>
    /// Gets the <see cref="IEqualityComparer"/> object that is used to determine equality for the values in this instance.
    /// </summary>
    /// <value>The <see cref="IEqualityComparer"/> object that is used to determine equality for the values in this instance.</value>
    IEqualityComparer Comparer { get; }

    /// <summary>
    /// Adds the specified element to the <see cref="ISet"/>.
    /// </summary>
    /// <param name="item">Item to add to the set.</param>
    /// <returns><see langword="true"/>if the element is added to the <see cref="ISet"/> object; false if the element is already present.</returns>
    bool Add(object item);

    /// <summary>
    /// Removes the specified item from this instance.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns><see langword="true"/> if the element is successfully found and removed; otherwise, <see langword="false"/>.</returns>
    bool Remove(object item);

    /// <summary>
    /// Removes all elements that match the conditions defined by the specified predicate from a <see cref="ISet"/> collection.
    /// </summary>
    /// <param name="match">The <see cref="Predicate{T}"/> delegate that defines the conditions of the elements to remove.</param>
    /// <returns>The number of elements that were removed from the <see cref="ISet"/> collection.</returns>
    int RemoveWhere(Predicate<object> match);

    /// <summary>
    /// Clears this instance.
    /// </summary>
    void Clear();

    /// <summary>
    /// Determines whether this instance contains the specified item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>
    ///   <see langword="true"/> if this instance contains the specified item; otherwise, <see langword="false"/>.
    /// </returns>
    bool Contains(object item);

    /// <summary>
    /// Determines if this instance is disjoint with the specified set.
    /// </summary>
    /// <param name="other">Set to check disjointness with.</param>
    /// <returns><see langword="true"/> if the two sets are disjoint; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
    /// <remarks>Two sets are disjoint if no item from one set is equal to any item from the other set.</remarks>
    bool IsDisjointWith(IEnumerable other);

    /// <summary>
    /// Determines if this instance is equal to another set. This set is equal to otherSet if they contain the same items.
    /// </summary>
    /// <param name="other">The collection to compare to this instance.</param>
    /// <returns><see langword="true"/> if this set is equal to otherSet; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
    bool SetEquals(IEnumerable other);

    /// <summary>
    /// Determines if this instance is a proper subset of the specified set.
    /// </summary>
    /// <param name="other">The collection to compare to this instance.</param>
    /// <returns><see langword="true"/> if this instance is a proper subset of <paramref name="other"/> set.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
    /// <remarks>One set is a subset of second set if every element in first set is also contained in second set. 
    /// Additionally, first set must have strictly fewer items than second set.</remarks>
    bool IsProperSubsetOf(IEnumerable other);

    /// <summary>
    /// Determines if this instance is a proper superset of the specified set.
    /// </summary>
    /// <param name="other">The collection to compare to this instance.</param>
    /// <returns><see langword="true"/> if this instance is the proper superset of the specified set; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
    /// <remarks>One set is the proper superset of second set if every element in second set is also contained in the first set. 
    /// Additionally, first set must have strictly more items than second set.</remarks>
    bool IsProperSupersetOf(IEnumerable other);

    /// <summary>
    /// Determines if this instance is a subset of the specified set.
    /// </summary>
    /// <param name="other">The collection to compare to this instance.</param>
    /// <returns><see langword="true"/> if this instance is a proper subset of <paramref name="other"/> set.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
    /// <remarks>One set is a subset of second set if every element in first set is also contained in second set.</remarks>
    bool IsSubsetOf(IEnumerable other);

    /// <summary>
    /// Determines if this instance is a superset of the specified set.
    /// </summary>
    /// <param name="other">The collection to compare to this instance.</param>
    /// <returns><see langword="true"/> if this instance is the proper superset of the specified set; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
    /// <remarks>One set is the proper superset of second set if every element in second set is also contained in the first set.</remarks>
    bool IsSupersetOf(IEnumerable other);

    /// <summary>
    /// Determines whether this instance overlaps the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to this instance.</param>
    /// <returns><see langword="true"/> if this instance and <paramref name="other"/> share at least one common element; otherwise, <see langword="false"/>.</returns>
    bool Overlaps(IEnumerable other);

    /// <summary>
    /// Modifies this instance to contain all elements that are present in both itself and in the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to this instance.</param>
    /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
    void UnionWith(IEnumerable other);

    /// <summary>
    /// Modifies this instance to contain only elements that are present in itself and in the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to this instance.</param>
    /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
    void IntersectWith(IEnumerable other);

    /// <summary>
    /// Removes all elements in the specified collection from this instance.
    /// </summary>
    /// <param name="other">The collection of items to remove from this instance.</param>
    /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
    void ExceptWith(IEnumerable other);

    /// <summary>
    /// Modifies this instance to contain only elements that are present in either itself, or in the specified collection, but not both.
    /// </summary>
    /// <param name="other">The collection to compare to this instance.</param>
    /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
    void SymmetricExceptWith(IEnumerable other);
  }
}