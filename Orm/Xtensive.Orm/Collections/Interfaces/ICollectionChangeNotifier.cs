// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.12

using System;
using System.Collections.Generic;

namespace Xtensive.Collections
{
  /// <summary>
  /// A collection exposing the collection change events contract.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  public interface ICollectionChangeNotifier<TItem>: IChangeNotifier
  {
    /// <summary>
    /// Occurs when collection is intended to be cleared.
    /// </summary>
    event EventHandler<ChangeNotifierEventArgs> Clearing;

    /// <summary>
    /// Occurs when collection was cleared.
    /// </summary>
    event EventHandler<ChangeNotifierEventArgs> Cleared;

    /// <summary>
    /// Occurs when collection validates new item.
    /// </summary>
    event EventHandler<CollectionChangeNotifierEventArgs<TItem>> Validate;

    /// <summary>
    /// Occurs when item is inserting into collection.
    /// </summary>
    event EventHandler<CollectionChangeNotifierEventArgs<TItem>> Inserting;

    /// <summary>
    /// Occurs when item was inserted into collection.
    /// </summary>
    event EventHandler<CollectionChangeNotifierEventArgs<TItem>> Inserted;

    /// <summary>
    /// Occurs when item is removing from collection.
    /// </summary>
    event EventHandler<CollectionChangeNotifierEventArgs<TItem>> Removing;

    /// <summary>
    /// Occurs when item was removed from collection.
    /// </summary>
    event EventHandler<CollectionChangeNotifierEventArgs<TItem>> Removed;

    /// <summary>
    /// Occurs when item is about to be changed.
    /// </summary>
    event EventHandler<CollectionChangeNotifierEventArgs<TItem>> ItemChanging;

    /// <summary>
    /// Occurs when item is changed.
    /// </summary>
    event EventHandler<CollectionChangeNotifierEventArgs<TItem>> ItemChanged;
  }
}