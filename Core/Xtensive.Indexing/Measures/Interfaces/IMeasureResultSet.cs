// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.02.16

using Xtensive.Collections;

namespace Xtensive.Indexing.Measures
{
  /// <summary>
  /// Represent a set of measurements for a <see cref="IMeasureSet{TItem}"/>.
  /// </summary>
  public interface IMeasureResultSet<TItem> : ICountable<IMeasure<TItem>>
  {
    /// <summary>
    /// Gets the <see cref="IMeasure{TItem}"/> at the specified index.
    /// </summary>
    /// <value>The <see cref="IMeasure{TItem}"/>.</value>
    IMeasure<TItem> this[int index] { get; }

    /// <summary>
    /// Gets the <see cref="IMeasure{TItem}"/> by specified name.
    /// </summary>
    /// <value>The <see cref="IMeasure{TItem}"/>.</value>
    IMeasure<TItem> this[string name] { get; }

    /// <summary>
    /// Gets a value indicating whether this instance is consistent - i.e. all 
    /// contained <see cref="IMeasure{TItem}"/> instances have results.
    /// </summary>
    /// <value>
    ///   <see langword="True"/> if this instance is consistent; otherwise, <see langword="false"/>.
    /// </value>
    bool IsConsistent { get; }

    /// <summary>
    /// Adds the specified item to all measure results.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns><see langword="true"/> if all measure results were updated successfully 
    /// (<see cref="IMeasure{TItem}.HasResult"/> is <see langword="true"/>), 
    /// otherwise - <see langword="false"/>.</returns>
    bool Add(TItem item);

    /// <summary>
    /// Subtracts the specified item from all measure results.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns><see langword="true"/> if all measure results where updated successfully 
    /// (<see cref="IMeasure{TItem}.HasResult"/> is <see langword="true"/>), 
    /// otherwise - <see langword="false"/>.</returns>
    bool Subtract(TItem item);
    

    /// <summary>
    /// Resets this instance.
    /// </summary>
    void Reset();
  }
}