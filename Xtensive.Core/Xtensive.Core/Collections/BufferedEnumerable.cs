// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.11.16

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Collections
{
  public class BufferedEnumerable<TItem>: ICountable<TItem>
  {
    private readonly ICollection<TItem> buffer;

    /// <inheritdoc/>
    [DebuggerHidden]
    public long Count
    {
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

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The source.</param>
    public BufferedEnumerable(ICollection<TItem> source)
    {
      buffer = source;
    }
  }
}