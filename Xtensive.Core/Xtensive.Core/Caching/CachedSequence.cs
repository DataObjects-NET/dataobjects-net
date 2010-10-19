// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.18

using System.Collections;
using System.Collections.Generic;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Caching
{
  /// <summary>
  /// Cached sequence wrapper.
  /// Wraps a cached value containing a sequence and exposes 
  /// it as <see cref="IEnumerable{T}"/>.
  /// </summary>
  /// <typeparam name="T">The type of element in sequence.</typeparam>
  public sealed class CachedSequence<T> : 
    ICachedValue<IEnumerable<T>>, 
    IEnumerable<T>
  {
    private readonly ICachedValue<IEnumerable<T>> cachedSequence;

    #region ICachedValue<IEnumerable<T>> members

    /// <inheritdoc/>
    public IEnumerable<T> Value
    {
      get { return cachedSequence.Value; }
    }

    /// <inheritdoc/>
    public bool IsActual
    {
      get { return cachedSequence.IsActual; }
    }

    /// <inheritdoc/>
    public void Invalidate()
    {
      cachedSequence.Invalidate();
    }

    #endregion

    #region IEnumerable<T> members

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return Value.GetEnumerator();
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
      return Value.GetEnumerator();
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="cachedSequence">The sequence to cache.</param>
    public CachedSequence(ICachedValue<IEnumerable<T>> cachedSequence)
    {
      this.cachedSequence = cachedSequence;
    }
  }
}