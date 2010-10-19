// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.14

using System;
using Xtensive.Collections;

namespace Xtensive.Indexing.Measures
{
  /// <summary>
  /// Configurable measurable - an editable dictionary-like collection 
  /// of <see cref="IMeasure{TItem,TResult}"/> objects representing measure definitions.
  /// </summary>
  /// <typeparam name="TItem">The type of item of underlying <see cref="ICountable{T}"/>.</typeparam>
  public interface IMeasurable<TItem>: ICountable<TItem>
  {
    /// <summary>
    /// Gets result indicating whether this instance contains measures.
    /// </summary>
    bool HasMeasures { get;}

    /// <summary>
    /// Gets the set of measures.
    /// </summary>
    IMeasureSet<TItem> Measures { get; }

    /// <summary>
    /// Gets the measurement for <see cref="IMeasure{TItem,TResult}"/> with the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name of the measure.</param>
    /// <returns>The measurement result.</returns>
    object GetMeasureResult(string name);

    /// <summary>
    /// Gets the measurements for a set of <see cref="IMeasure{TItem,TResult}"/> with the specified <paramref name="names"/>.
    /// </summary>
    /// <param name="names">The names of measures to get.</param>
    /// <returns>The <see cref="Array"/> of measurement results for specified <see cref="IMeasure{TItem,TResult}"/>s.</returns>
    object[] GetMeasureResults(params string[] names);
  }
}