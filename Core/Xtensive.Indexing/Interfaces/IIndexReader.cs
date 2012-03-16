// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.02.14

using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Indexing
{
  /// <summary>
  /// An extended version of <see cref="IEnumerator{T}"/> for indexes.
  /// </summary>
  /// <typeparam name="TKey">The type of index key.</typeparam>
  /// <typeparam name="TItem">The type of index item.</typeparam>
  public interface IIndexReader<TKey, TItem> : 
    IEnumerable<TItem>,
    IEnumerator<TItem>
  {
    /// <summary>
    /// Gets the range of enumeration.
    /// </summary>
    Range<Entire<TKey>> Range { get; }

    /// <summary>
    /// Gets the direction of enumeration relatively to index key comparer.
    /// </summary>
    Direction Direction { get; }

    /// <summary>
    /// Moves the internal pointer to the specified key.
    /// You still should call <see cref="IEnumerator.MoveNext"/>
    /// after calling this method to make 
    /// <see cref="IEnumerator{T}.Current"/> property value available.
    /// </summary>
    /// <param name="key">The key to move to.</param>
    /// <remarks>
    /// <note type="caution">
    ///   The specified <paramref name="key"/> must be 
    ///   inside the <see cref="Range"/> of this reader.
    /// </note>
    ///  </remarks>
    void MoveTo(Entire<TKey> key);
  }
}