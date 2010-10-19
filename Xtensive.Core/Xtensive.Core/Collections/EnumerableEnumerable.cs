// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.11.16

using System.Collections;
using System.Collections.Generic;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Collections
{
  /// <summary>
  /// Enumerable of enumerables flattener.
  /// </summary>
  /// <typeparam name="TSource">The type of enumerable items.</typeparam>
  /// <typeparam name="TItem">The type of item in the enumerable of enumerables.</typeparam>
  public class EnumerableEnumerable<TSource,TItem>: IEnumerable<TItem>
    where TSource : IEnumerable<TItem>
  {
    private readonly IEnumerable<TSource> source;

    #region IEnumerable<TItem> Members

    /// <inheritdoc/>
    IEnumerator<TItem> IEnumerable<TItem>.GetEnumerator()
    {
      foreach (TSource enumerable in source)
        foreach (TItem item in enumerable)
          yield return item;
    }

    /// <inheritdoc/>
    public IEnumerator GetEnumerator()
    {
      return ((IEnumerable<TItem>)this).GetEnumerator();
    }

    #endregion

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="source">The source to flatten.</param>
    public EnumerableEnumerable(IEnumerable<TSource> source)
    {
      this.source = source;
    }
  }
}