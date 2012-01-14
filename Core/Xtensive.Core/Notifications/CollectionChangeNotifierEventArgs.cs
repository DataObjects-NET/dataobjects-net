// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.03.01

using System;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Notifications
{
  /// <summary>
  /// Represents class that contains <see cref="ICollectionChangeNotifier{TItem}"/> related event data.
  /// </summary>
  [Serializable]
  public sealed class CollectionChangeNotifierEventArgs<TItem> : EventArgs
  {
    private readonly int? index;
    private readonly TItem item;

    /// <summary>
    /// Gets the item's index if any.
    /// </summary>
    /// <value>The index.</value>
    public int? Index
    {
      get { return index; }
    }

    /// <summary>
    /// Gets the item.
    /// </summary>
    /// <value>The item.</value>
    public TItem Item
    {
      get { return item; }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="index">The index.</param>
    public CollectionChangeNotifierEventArgs(TItem item, int index)
    {
      this.index = index;
      this.item = item;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="item">The item.</param>
    public CollectionChangeNotifierEventArgs(TItem item)
    {
      this.item = item;
    }
  }
}
