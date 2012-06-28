// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.07.02

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Collections
{
  /// <summary>
  /// Contract for collection classifying all the items by their classes.
  /// </summary>
  /// <typeparam name="TClass">The type of the class.</typeparam>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  public interface IClassifiedCollection<TClass, TItem> : ICollection<TItem>
  {
    /// <summary>
    /// Gets a value indicating whether this instance behaves like a set.
    /// </summary>
    bool IsSet { get; }

    /// <summary>
    /// Gets the count of classes.
    /// </summary>
    int ClassCount { get; }

    /// <summary>
    /// Gets the classifier function.
    /// </summary>
    Func<TItem, TClass[]> Classifier { get; }

    /// <summary>
    /// Adds the range of items.
    /// </summary>
    /// <param name="items">The items to add.</param>
    void AddRange(IEnumerable<TItem> items);

    /// <summary>
    /// Gets all the items of the specified class.
    /// </summary>
    /// <param name="class">The class of items to get.</param>
    /// <returns>A sequence of items of the specified class.</returns>
    IEnumerable<TItem> GetItems(TClass @class);

    /// <summary>
    /// Gets the classes.
    /// </summary>
    IEnumerable<TClass> GetClasses();
  }
}