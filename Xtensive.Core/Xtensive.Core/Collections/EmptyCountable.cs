// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.11.22

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xtensive.Collections
{
  /// <summary>
  /// Empty <see cref="ICountable{TItem}"/> implementation.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  public sealed class EmptyCountable<TItem> : ICountable<TItem>
  {
    /// <inheritdoc/>
    public long Count
    {
      [DebuggerStepThrough]
      get { return 0; }
    }

    #region IEnumerable<...> methods

    /// <inheritdoc/>
    [DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    public IEnumerator<TItem> GetEnumerator()
    {
      yield break;
    }

    #endregion
  }
}