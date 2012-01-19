// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.14

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Collections
{
  /// <summary>
  /// Describes complete <see cref="DifferentialDictionary{TKey,TValue}"/> change set.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("Count = {ChangedItems.Count} (+ {AddedItems.Count}, - {RemovedItems.Count})")]
  public sealed class DifferentialDictionaryDifference<TKey, TValue>
  {
    /// <summary>
    /// Gets added items.
    /// </summary>
    public ReadOnlyDictionary<TKey, TValue> AddedItems { get; private set; }

    /// <summary>
    /// Gets removed items.
    /// </summary>
    public ReadOnlyDictionary<TKey, TValue> RemovedItems { get; private set; }

    /// <summary>
    /// Gets the keys of all changed items, including keys of added, removed or changed items.
    /// </summary>
    public ReadOnlyHashSet<TKey> ChangedItems { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="addedItems">The added items.</param>
    /// <param name="removedItems">The removed items.</param>
    /// <param name="changedItems">The changed items.</param>
    public DifferentialDictionaryDifference(
      ReadOnlyDictionary<TKey, TValue> addedItems, 
      ReadOnlyDictionary<TKey, TValue> removedItems, 
      ReadOnlyHashSet<TKey> changedItems)
    {
      AddedItems = addedItems;
      RemovedItems = removedItems;
      ChangedItems = changedItems;
    }
  }
}