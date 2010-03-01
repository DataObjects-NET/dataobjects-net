// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.11.14

using System;

namespace Xtensive.Indexing.Measures
{
  /// <summary>
  /// Defines measure for some set of items - e.g. count of items.
  /// </summary>
  /// <typeparam name="TItem">The type of item in item collection this measure is defined for.</typeparam>
  public interface IMeasure<TItem>
  {
    /// <summary>
    /// Gets result of the measurement.
    /// </summary>
    object Result { get; }

    /// <summary>
    /// Gets name of the current measure.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets a value indicating whether this instance has value.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if this instance has value; otherwise, <see langword="false"/>.
    /// </value>
    bool HasResult { get; }

    /// <summary>
    /// Resets result of measurement to its default state.
    /// </summary>
    void Reset();

    /// <summary>
    /// Adds the specified item to the current measure.
    /// </summary>
    /// <param name="item">The item.</param>
    bool Add(TItem item);

    /// <summary>
    /// Removes the specified item from the current measure.
    /// </summary>
    /// <param name="item">The item.</param>
    bool Subtract(TItem item);

    /// <summary>
    /// Adds measure to the current one and returns new measure.
    /// </summary>
    /// <remarks>Does not modify current measure.</remarks>
    /// <param name="measure">The measure to add.</param>
    IMeasure<TItem> Add(IMeasure<TItem> measure);

    /// <summary>
    /// Subtracts measure from the current one and returns new measure.
    /// </summary>
    /// <remarks>Does not modify current measure.</remarks>
    /// <param name="measure">The measure to subtract.</param>
    IMeasure<TItem> Subtract(IMeasure<TItem> measure);

    /// <summary>
    /// Adds measure to the current one.
    /// </summary>
    /// <param name="measure">The measure to add.</param>
    bool AddWith(IMeasure<TItem> measure);

    /// <summary>
    /// Subtracts measure from the current one.
    /// </summary>
    /// <param name="measure">The measure to subtract.</param>
    bool SubtractWith(IMeasure<TItem> measure);

    /// <summary>
    /// Creates new instance of current measure.
    /// </summary>
    IMeasure<TItem> CreateNew();

    /// <summary>
    /// Creates new instance of current measure with the specified name.
    /// </summary>
    /// <param name="newName">The new measure name.</param>
    IMeasure<TItem> CreateNew(string newName);
  }
}
