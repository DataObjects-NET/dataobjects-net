// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.22

using System;
using System.Collections;
using System.Diagnostics;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Base class for any <see cref="CollectionIndex{TKey,TItem}"/>.
  /// </summary>
  [Serializable]
  public abstract class CollectionIndexBase: IIndex
  {
    /// <summary>
    /// Gets the name of the index.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the underlying index.
    /// </summary>
    protected abstract IIndex Index { get; }

    #region IIndex members

    /// <inheritdoc/>
    public long Count
    {
      [DebuggerStepThrough]
      get { return Index.Count; }
    }

    /// <inheritdoc/>
    public IEnumerator GetEnumerator()
    {
      return Index.GetEnumerator();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Thrown always.</exception>
    void IIndex.Clear()
    {
      throw new NotSupportedException();
    }

    #endregion
  }
}