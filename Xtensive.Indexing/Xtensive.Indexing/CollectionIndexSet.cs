// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.11.08

using System;
using Xtensive.Configuration;

namespace Xtensive.Indexing
{
  /// <summary>
  /// A set of indexes that serves the collection of <typeparamref name="TItem"/>s.
  /// </summary>
  /// <typeparam name="TItem">The type of collection items.</typeparam>
  [Serializable]
  public sealed class CollectionIndexSet<TItem>: ConfigurationSetBase<CollectionIndexBase>
  {
    #region IIndexSet<TItem> Members

    /// <inheritdoc/>
    public IIndex<TKey, TItem> GetItem<TKey>()
    {
      // TODO: Remark that first found item is returned
      foreach (CollectionIndexBase index in this) {
        IIndex<TKey, TItem> typedIndex = index as IIndex<TKey, TItem>;
        if (typedIndex!=null)
          return typedIndex;
      }
      return null;
    }

    #endregion

    /// <inheritdoc/>
    protected override string GetItemName(CollectionIndexBase item)
    {
      return item.Name;
    }

    #region Clone implementation

    /// <inheritdoc/>
    protected override ConfigurationBase CreateClone()
    {
      throw new NotSupportedException();
    }

    #endregion


    // Constructors

    /// <inheritdoc/>
    public CollectionIndexSet()
    {
    }

    /// <inheritdoc/>
    public CollectionIndexSet(CollectionIndexBase item)
      : base(item)
    {
    }

    /// <inheritdoc/>
    public CollectionIndexSet(CollectionIndexBase item, params CollectionIndexBase[] items)
      : base(item, items)
    {
    }
  }
}