// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.11.22

using System.Collections;
using System.Collections.Generic;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// Dummy <see cref="ICountable{TItem}"/> implementation.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  public sealed class DummyCountable<TItem> : ICountable<TItem>
  {
    /// <inheritdoc/>
    public long Count
    {
      get { return 0; }
    }

    /// <inheritdoc/>
    public IEnumerator<TItem> GetEnumerator()
    {
      yield break;
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable<TItem>)this).GetEnumerator();
    }
  }
}