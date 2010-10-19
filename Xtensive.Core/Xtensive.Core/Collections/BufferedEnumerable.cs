// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.11.16

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Collections
{
  /// <summary>
  /// Caches passed <see cref="IEnumerable{T}"/> inside internal
  /// <see cref="List{T}"/> on construction, and uses this list
  /// on subsequent enumerations.
  /// So the original sequence is enumerated just once.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  public class BufferedEnumerable<TItem>: ICountable<TItem>
  {
    private readonly ICollection<TItem> buffer;

    /// <inheritdoc/>
    public long Count {
      [DebuggerStepThrough]
      get { return buffer.Count; }
    }

    /// <inheritdoc/>
    IEnumerator<TItem> IEnumerable<TItem>.GetEnumerator()
    {
      return buffer.GetEnumerator();
    }

    /// <inheritdoc/>
    public IEnumerator GetEnumerator()
    {
      return ((IEnumerable<TItem>)this).GetEnumerator();
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="source">The source.</param>
    public BufferedEnumerable(IEnumerable<TItem> source)
    {
      buffer = new List<TItem>(source);
    }
  }
}